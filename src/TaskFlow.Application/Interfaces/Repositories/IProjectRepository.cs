using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdForUserAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(Project project, CancellationToken cancellationToken);

    Task DeleteAsync(Project project, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
