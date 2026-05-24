using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class DashboardRepository(TaskFlowDbContext dbContext) : IDashboardRepository
{
    public async Task<DashboardSummaryResponse> GetSummaryForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var totalProjects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.OwnerId == userId)
            .CountAsync(cancellationToken);

        var tasksQuery = dbContext.TaskItems
            .AsNoTracking()
            .Where(task => task.Project.OwnerId == userId);

        var totalTasks = await tasksQuery.CountAsync(cancellationToken);

        var statusCounts = await tasksQuery
            .GroupBy(task => task.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);

        var today = DateTime.UtcNow.Date;
        var overdueTasks = await tasksQuery.CountAsync(
            task => task.DueDate != null
                && task.DueDate.Value.Date < today
                && task.Status != TaskItemStatus.Done,
            cancellationToken);

        return new DashboardSummaryResponse(
            totalProjects,
            totalTasks,
            statusCounts.GetValueOrDefault(TaskItemStatus.Todo),
            statusCounts.GetValueOrDefault(TaskItemStatus.InProgress),
            statusCounts.GetValueOrDefault(TaskItemStatus.Done),
            overdueTasks);
    }
}
