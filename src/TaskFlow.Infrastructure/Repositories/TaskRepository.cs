using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class TaskRepository(TaskFlowDbContext dbContext) : ITaskRepository
{
    public Task<TaskItem?> GetByIdForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken) =>
        dbContext.TaskItems
            .FirstOrDefaultAsync(
                task => task.Id == taskId && task.Project.OwnerId == userId,
                cancellationToken);

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetFilteredForUserAsync(
        Guid userId,
        TaskFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var query = BuildFilteredQuery(userId, filter);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(task => task.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await dbContext.TaskItems.AddAsync(task, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken)
    {
        dbContext.TaskItems.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<TaskItem> BuildFilteredQuery(Guid userId, TaskFilterRequest filter)
    {
        IQueryable<TaskItem> query = dbContext.TaskItems
            .AsNoTracking()
            .Where(task => task.Project.OwnerId == userId);

        if (filter.ProjectId.HasValue)
        {
            query = query.Where(task => task.ProjectId == filter.ProjectId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(task => task.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(task => task.Priority == filter.Priority.Value);
        }

        if (filter.OverdueOnly)
        {
            var today = DateTime.UtcNow.Date;
            query = query.Where(task =>
                task.DueDate != null
                && task.DueDate.Value.Date < today
                && task.Status != TaskItemStatus.Done);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(task =>
                task.Title.Contains(search)
                || (task.Description != null && task.Description.Contains(search)));
        }

        return query;
    }
}
