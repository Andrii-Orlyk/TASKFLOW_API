using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<TaskComment?> GetByIdForUserAsync(Guid commentId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskComment>> GetByTaskIdForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken);

    Task AddAsync(TaskComment comment, CancellationToken cancellationToken);

    Task DeleteAsync(TaskComment comment, CancellationToken cancellationToken);
}
