using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Tasks;

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate);
