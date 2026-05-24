using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Api.Models;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Auth;

public sealed class AuthIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_ShouldReturnToken_WhenDataIsValid()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("user@example.com", "Password123!", "Jane", "Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var client = CreateClient();
        const string email = "login@example.com";
        const string password = "Password123!";

        await client.RegisterUserAsync(email, password);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Me_ShouldReturnCurrentUser_WhenTokenIsValid()
    {
        var client = CreateClient();
        const string email = "me@example.com";

        var token = await client.RegisterUserAsync(email);
        client.UseBearerToken(token);

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(TestJsonOptions.Default);
        user!.Email.Should().Be(email);
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
    }

    [Fact]
    public async Task Register_ShouldReturn409_WhenEmailAlreadyExists()
    {
        var client = CreateClient();
        const string email = "duplicate@example.com";

        await client.RegisterUserAsync(email);

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, "Password123!", "Jane", "Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("auth.email_exists");
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenPasswordIsInvalid()
    {
        var client = CreateClient();
        const string email = "wrong-password@example.com";

        await client.RegisterUserAsync(email);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, "WrongPassword123!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("auth.invalid_credentials");
    }

    [Fact]
    public async Task Me_ShouldReturn401WithApiErrorResponse_WhenTokenMissing()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error.Should().NotBeNull();
        error!.StatusCode.Should().Be(401);
        error.Code.Should().Be("auth.unauthorized");
        error.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenEmailIsInvalid()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("not-an-email", "Password123!", "Jane", "Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("validation.failed");
        error.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenPasswordIsTooShort()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("short-pw@example.com", "abc", "Jane", "Doe"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("validation.failed");
        error.Errors.Should().NotBeEmpty();
    }
}
