using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class CommentRepository(TaskFlowDbContext dbContext) : ICommentRepository
{
    public Task<TaskComment?> GetByIdForUserAsync(
        Guid commentId,
        Guid userId,
        CancellationToken cancellationToken) =>
        dbContext.TaskComments
            .FirstOrDefaultAsync(
                comment => comment.Id == commentId && comment.TaskItem.Project.OwnerId == userId,
                cancellationToken);

    public async Task<IReadOnlyList<TaskComment>> GetByTaskIdForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken) =>
        await dbContext.TaskComments
            .AsNoTracking()
            .Where(comment =>
                comment.TaskItemId == taskId
                && comment.TaskItem.Project.OwnerId == userId)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TaskComment comment, CancellationToken cancellationToken)
    {
        await dbContext.TaskComments.AddAsync(comment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TaskComment comment, CancellationToken cancellationToken)
    {
        dbContext.TaskComments.Remove(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
