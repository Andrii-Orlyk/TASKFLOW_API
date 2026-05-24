using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Factories;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Application.Policies;
using TaskFlow.Application.Strategies;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository,
    ICurrentUserService currentUserService,
    ITaskFactory taskFactory,
    ITaskPriorityStrategy priorityStrategy,
    ITaskStatusPolicy statusPolicy) : ITaskService
{
    private const string UnauthorizedCode = "auth.unauthorized";
    private const string UnauthorizedMessage = "User is not authenticated.";
    private const string ProjectNotFoundCode = "project.not_found";
    private const string ProjectNotFoundMessage = "Project not found.";
    private const string TaskNotFoundCode = "task.not_found";
    private const string TaskNotFoundMessage = "Task not found.";
    private const string InvalidDueDateCode = "task.invalid_due_date";
    private const string InvalidDueDateMessage = "Due date cannot be in the past.";
    private const string InvalidStatusTransitionCode = "task.invalid_status_transition";
    private const string InvalidStatusTransitionMessage = "Task status transition is not allowed.";

    public async Task<Result<PagedResult<TaskResponse>>> GetFilteredAsync(
        TaskFilterRequest filter,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<PagedResult<TaskResponse>>.Failure(
                Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var (items, totalCount) = await taskRepository.GetFilteredForUserAsync(userId, filter, cancellationToken);
        var pagedResult = PagedResult<TaskResponse>.Create(
            items.Select(MapToResponse).ToList(),
            filter.Page,
            filter.PageSize,
            totalCount);

        return Result<PagedResult<TaskResponse>>.Success(pagedResult);
    }

    public async Task<Result<TaskResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<TaskResponse>.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var task = await taskRepository.GetByIdForUserAsync(id, userId, cancellationToken);

        if (task is null)
        {
            return Result<TaskResponse>.Failure(Error.Create(TaskNotFoundCode, TaskNotFoundMessage));
        }

        return Result<TaskResponse>.Success(MapToResponse(task));
    }

    public async Task<Result<TaskResponse>> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<TaskResponse>.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        if (IsDueDateInPast(request.DueDate))
        {
            return Result<TaskResponse>.Failure(Error.Create(InvalidDueDateCode, InvalidDueDateMessage));
        }

        var project = await projectRepository.GetByIdForUserAsync(request.ProjectId, userId, cancellationToken);

        if (project is null)
        {
            return Result<TaskResponse>.Failure(Error.Create(ProjectNotFoundCode, ProjectNotFoundMessage));
        }

        var priority = priorityStrategy.Resolve(request.Priority, request.DueDate);
        var task = taskFactory.Create(
            request.ProjectId,
            request.Title,
            request.Description,
            priority,
            request.DueDate);

        await taskRepository.AddAsync(task, cancellationToken);

        return Result<TaskResponse>.Success(MapToResponse(task));
    }

    public async Task<Result<TaskResponse>> UpdateAsync(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<TaskResponse>.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        if (IsDueDateInPast(request.DueDate))
        {
            return Result<TaskResponse>.Failure(Error.Create(InvalidDueDateCode, InvalidDueDateMessage));
        }

        var task = await taskRepository.GetByIdForUserAsync(id, userId, cancellationToken);

        if (task is null)
        {
            return Result<TaskResponse>.Failure(Error.Create(TaskNotFoundCode, TaskNotFoundMessage));
        }

        task.Title = request.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        task.Priority = priorityStrategy.Resolve(request.Priority, request.DueDate);
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(MapToResponse(task));
    }

    public async Task<Result<TaskResponse>> UpdateStatusAsync(
        Guid id,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<TaskResponse>.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var task = await taskRepository.GetByIdForUserAsync(id, userId, cancellationToken);

        if (task is null)
        {
            return Result<TaskResponse>.Failure(Error.Create(TaskNotFoundCode, TaskNotFoundMessage));
        }

        if (!statusPolicy.CanTransition(task.Status, request.Status))
        {
            return Result<TaskResponse>.Failure(
                Error.Create(InvalidStatusTransitionCode, InvalidStatusTransitionMessage));
        }

        ApplyStatusChange(task, request.Status);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(MapToResponse(task));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var task = await taskRepository.GetByIdForUserAsync(id, userId, cancellationToken);

        if (task is null)
        {
            return Result.Failure(Error.Create(TaskNotFoundCode, TaskNotFoundMessage));
        }

        await taskRepository.DeleteAsync(task, cancellationToken);

        return Result.Success();
    }

    internal static void ApplyStatusChange(TaskItem task, TaskItemStatus newStatus)
    {
        if (task.Status == TaskItemStatus.Done && newStatus != TaskItemStatus.Done)
        {
            task.CompletedAt = null;
        }

        if (newStatus == TaskItemStatus.Done)
        {
            task.CompletedAt = DateTime.UtcNow;
        }

        task.Status = newStatus;
        task.UpdatedAt = DateTime.UtcNow;
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

    private static bool IsDueDateInPast(DateTime? dueDate) =>
        dueDate is not null && dueDate.Value.Date < DateTime.UtcNow.Date;

    private static TaskResponse MapToResponse(TaskItem task) =>
        new(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.CompletedAt,
            task.CreatedAt,
            task.UpdatedAt);
}
