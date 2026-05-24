using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ICurrentUserService currentUserService) : IAuthService
{
    private const string InvalidCredentialsCode = "auth.invalid_credentials";
    private const string InvalidCredentialsMessage = "Invalid email or password.";
    private const string EmailExistsCode = "auth.email_exists";
    private const string EmailExistsMessage = "Email is already registered.";
    private const string UnauthorizedCode = "auth.unauthorized";
    private const string UnauthorizedMessage = "User is not authenticated.";

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return Result<AuthResponse>.Failure(Error.Create(EmailExistsCode, EmailExistsMessage));
        }

        var utcNow = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.User,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await userRepository.AddAsync(user, cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(jwtTokenService.GenerateToken(user)));
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(
                Error.Create(InvalidCredentialsCode, InvalidCredentialsMessage));
        }

        return Result<AuthResponse>.Success(new AuthResponse(jwtTokenService.GenerateToken(user)));
    }

    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is not Guid userId)
        {
            return Result<CurrentUserResponse>.Failure(
                Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<CurrentUserResponse>.Failure(
                Error.Create(UnauthorizedCode, UnauthorizedMessage));
        }

        return Result<CurrentUserResponse>.Success(MapToCurrentUserResponse(user));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static CurrentUserResponse MapToCurrentUserResponse(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Role);
}
