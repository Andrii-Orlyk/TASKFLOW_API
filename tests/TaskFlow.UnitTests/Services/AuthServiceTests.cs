using FluentAssertions;
using Moq;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService(
            _userRepository.Object,
            _passwordHasher.Object,
            _jwtTokenService.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        var request = new RegisterRequest("user@example.com", "password123", "John", "Doe");
        _userRepository
            .Setup(repository => repository.ExistsByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("auth.email_exists");
        _userRepository.Verify(
            repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsInvalid()
    {
        var user = CreateUser();
        _userRepository
            .Setup(repository => repository.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(hasher => hasher.Verify("wrong-password", user.PasswordHash))
            .Returns(false);

        var result = await _authService.LoginAsync(
            new LoginRequest(user.Email, "wrong-password"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("auth.invalid_credentials");
        result.Error.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnToken_WhenRegistrationIsValid()
    {
        var request = new RegisterRequest("user@example.com", "password123", "John", "Doe");
        _userRepository
            .Setup(repository => repository.ExistsByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher
            .Setup(hasher => hasher.Hash(request.Password))
            .Returns("hashed-password");
        _jwtTokenService
            .Setup(service => service.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var result = await _authService.RegisterAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        _userRepository.Verify(
            repository => repository.AddAsync(It.Is<User>(user =>
                user.Email == "user@example.com"
                && user.PasswordHash == "hashed-password"
                && user.Role == UserRole.User), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static User CreateUser() =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
}
