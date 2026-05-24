using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class ProjectRepository(TaskFlowDbContext dbContext) : IProjectRepository
{
    public Task<Project?> GetByIdForUserAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken) =>
        dbContext.Projects
            .FirstOrDefaultAsync(
                project => project.Id == projectId && project.OwnerId == userId,
                cancellationToken);

    public async Task<IReadOnlyList<Project>> GetAllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.OwnerId == userId)
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Project project, CancellationToken cancellationToken)
    {
        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
