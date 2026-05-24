namespace TaskFlow.Application.DTOs.Comments;

public record CommentResponse(
    Guid Id,
    Guid TaskItemId,
    Guid AuthorId,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);
