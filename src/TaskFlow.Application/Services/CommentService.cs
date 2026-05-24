using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Comments;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Services;

public sealed class CommentService(
    ICommentRepository commentRepository,
    ITaskRepository taskRepository,
    ICurrentUserService currentUserService) : ICommentService
{
    private const string UnauthorizedCode = "auth.unauthorized";
    private const string UnauthorizedMessage = "User is not authenticated.";
    private const string TaskNotFoundCode = "task.not_found";
    private const string TaskNotFoundMessage = "Task not found.";
    private const string CommentNotFoundCode = "comment.not_found";
    private const string CommentNotFoundMessage = "Comment not found.";
    private const string EmptyContentCode = "comment.empty_content";
    private const string EmptyContentMessage = "Comment content is required.";

    public async Task<Result<IReadOnlyList<CommentResponse>>> GetByTaskIdAsync(
        Guid taskId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<IReadOnlyList<CommentResponse>>.Failure(
                Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        if (!await TaskExistsForUserAsync(taskId, userId, cancellationToken))
        {
            return Result<IReadOnlyList<CommentResponse>>.Failure(
                Error.Create(TaskNotFoundCode, TaskNotFoundMessage));
        }

        var comments = await commentRepository.GetByTaskIdForUserAsync(taskId, userId, cancellationToken);
        return Result<IReadOnlyList<CommentResponse>>.Success(
            comments.Select(MapToResponse).ToList());
    }

    public async Task<Result<CommentResponse>> CreateAsync(
        Guid taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result<CommentResponse>.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return Result<CommentResponse>.Failure(Error.Create(EmptyContentCode, EmptyContentMessage));
        }

        if (!await TaskExistsForUserAsync(taskId, userId, cancellationToken))
        {
            return Result<CommentResponse>.Failure(Error.Create(TaskNotFoundCode, TaskNotFoundMessage));
        }

        var utcNow = DateTime.UtcNow;
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId,
            AuthorId = userId,
            Content = request.Content.Trim(),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await commentRepository.AddAsync(comment, cancellationToken);

        return Result<CommentResponse>.Success(MapToResponse(comment));
    }

    public async Task<Result> DeleteAsync(Guid commentId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Result.Failure(Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var comment = await commentRepository.GetByIdForUserAsync(commentId, userId, cancellationToken);

        if (comment is null)
        {
            return Result.Failure(Error.Create(CommentNotFoundCode, CommentNotFoundMessage));
        }

        await commentRepository.DeleteAsync(comment, cancellationToken);

        return Result.Success();
    }

    private async Task<bool> TaskExistsForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdForUserAsync(taskId, userId, cancellationToken);
        return task is not null;
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

    private static CommentResponse MapToResponse(TaskComment comment) =>
        new(
            comment.Id,
            comment.TaskItemId,
            comment.AuthorId,
            comment.Content,
            comment.CreatedAt,
            comment.UpdatedAt);
}
