namespace TaskFlow.Application.DTOs.Projects;

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);
