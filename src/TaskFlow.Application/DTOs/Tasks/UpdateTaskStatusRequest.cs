using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Tasks;

public record UpdateTaskStatusRequest(TaskItemStatus Status);
