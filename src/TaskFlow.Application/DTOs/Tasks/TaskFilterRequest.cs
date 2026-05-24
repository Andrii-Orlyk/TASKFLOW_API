using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Tasks;

public record TaskFilterRequest(
    Guid? ProjectId = null,
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    bool OverdueOnly = false,
    string? Search = null,
    int Page = 1,
    int PageSize = 10);
