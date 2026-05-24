using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
