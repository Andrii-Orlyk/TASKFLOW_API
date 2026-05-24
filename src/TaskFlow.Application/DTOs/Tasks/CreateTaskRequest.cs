using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Tasks;

public record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate);
