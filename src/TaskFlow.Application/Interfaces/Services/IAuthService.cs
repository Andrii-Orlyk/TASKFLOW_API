using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;

namespace TaskFlow.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<Result<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken);
}
