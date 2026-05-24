using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Auth;

public record CurrentUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role);
