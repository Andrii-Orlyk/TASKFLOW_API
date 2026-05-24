using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Services;

public sealed class ProjectService(
    IProjectRepository projectRepository,
    ICurrentUserService currentUserService) : IProjectService
{
    private const string UnauthorizedCode = "auth.unauthorized";
    private const string UnauthorizedMessage = "User is not authenticated.";
    private const string NotFoundCode = "project.not_found";
    private const string NotFoundMessage = "Project not found.";

    public async Task<Result<IReadOnlyList<ProjectResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<IReadOnlyList<ProjectResponse>>.Failure(
                Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var projects = await projectRepository.GetAllForUserAsync(userId, cancellationToken);
        return Result<IReadOnlyList<ProjectResponse>>.Success(
            projects.Select(MapToResponse).ToList());
    }

    public async Task<Result<ProjectResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<ProjectResponse>.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var project = await projectRepository.GetByIdForUserAsync(id, userId, cancellationToken);

        if (project is null)
        {
            return Result<ProjectResponse>.Failure(Error.Create(NotFoundCode, NotFoundMessage));
        }

        return Result<ProjectResponse>.Success(MapToResponse(project));
    }

    public async Task<Result<ProjectResponse>> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<ProjectResponse>.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<ProjectResponse>.Failure(
                Error.Create("project.invalid_name", "Project name is required."));
        }

        var utcNow = DateTime.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = NormalizeDescription(request.Description),
            OwnerId = userId,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await projectRepository.AddAsync(project, cancellationToken);

        return Result<ProjectResponse>.Success(MapToResponse(project));
    }

    public async Task<Result<ProjectResponse>> UpdateAsync(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<ProjectResponse>.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var project = await projectRepository.GetByIdForUserAsync(id, userId, cancellationToken);

        if (project is null)
        {
            return Result<ProjectResponse>.Failure(Error.Create(NotFoundCode, NotFoundMessage));
        }

        project.Name = request.Name.Trim();
        project.Description = NormalizeDescription(request.Description);
        project.UpdatedAt = DateTime.UtcNow;

        await projectRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectResponse>.Success(MapToResponse(project));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var project = await projectRepository.GetByIdForUserAsync(id, userId, cancellationToken);

        if (project is null)
        {
            return Result.Failure(Error.Create(NotFoundCode, NotFoundMessage));
        }

        await projectRepository.DeleteAsync(project, cancellationToken);

        return Result.Success();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        if (currentUserService.IsAuthenticated && currentUserService.UserId is Guid id)
        {
            userId = id;
            return true;
        }

        userId = Guid.Empty;
        return false;
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static ProjectResponse MapToResponse(Project project) =>
        new(project.Id, project.Name, project.Description, project.CreatedAt, project.UpdatedAt);
}
