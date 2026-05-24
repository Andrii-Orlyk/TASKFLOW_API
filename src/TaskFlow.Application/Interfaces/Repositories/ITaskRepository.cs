using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces.Repositories;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdForUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);

    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetFilteredForUserAsync(
        Guid userId,
        TaskFilterRequest filter,
        CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
