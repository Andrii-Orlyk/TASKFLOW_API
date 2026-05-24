using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Projects;

namespace TaskFlow.Application.Interfaces.Services;

public interface IProjectService
{
    Task<Result<IReadOnlyList<ProjectResponse>>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<ProjectResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<ProjectResponse>> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    Task<Result<ProjectResponse>> UpdateAsync(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
