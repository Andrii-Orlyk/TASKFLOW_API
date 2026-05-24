using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Comments;

namespace TaskFlow.Application.Interfaces.Services;

public interface ICommentService
{
    Task<Result<IReadOnlyList<CommentResponse>>> GetByTaskIdAsync(
        Guid taskId,
        CancellationToken cancellationToken);

    Task<Result<CommentResponse>> CreateAsync(
        Guid taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid commentId, CancellationToken cancellationToken);
}
