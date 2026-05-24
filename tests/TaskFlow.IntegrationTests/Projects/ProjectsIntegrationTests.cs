using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Api.Models;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Projects;

public sealed class ProjectsIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetProjects_ShouldReturnOnlyCurrentUserProjects()
    {
        var clientA = CreateClient();
        var clientB = CreateClient();

        var tokenA = await clientA.RegisterUserAsync("user-a@example.com");
        var tokenB = await clientB.RegisterUserAsync("user-b@example.com");

        clientA.UseBearerToken(tokenA);
        clientB.UseBearerToken(tokenB);

        await clientA.CreateProjectAsync("Project A");
        await clientB.CreateProjectAsync("Project B");

        var response = await clientA.GetAsync("/api/projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var projects = await response.Content.ReadFromJsonAsync<List<ProjectResponse>>();
        projects.Should().ContainSingle(project => project.Name == "Project A");
        projects.Should().NotContain(project => project.Name == "Project B");
    }

    [Fact]
    public async Task CreateProject_ShouldRequireAuthentication()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest("Unauthorized Project", null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProject_ShouldReturn400_WhenNameIsEmpty()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("empty-project@example.com");
        client.UseBearerToken(token);

        var response = await client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest("   ", null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        // Validator fires before service; FluentValidation NotEmpty/Must rule returns validation.failed.
        error!.Code.Should().Be("validation.failed");
        error.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProject_ShouldReturn404_WhenProjectBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("proj-get-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("proj-get-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Private Project");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.GetAsync($"/api/projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("project.not_found");
    }

    [Fact]
    public async Task UpdateProject_ShouldReturn404_WhenProjectBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("proj-upd-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("proj-upd-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Private Project");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.PutAsJsonAsync(
            $"/api/projects/{projectId}",
            new UpdateProjectRequest("Hijacked", null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("project.not_found");
    }

    [Fact]
    public async Task DeleteProject_ShouldReturn404_WhenProjectBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("proj-del-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("proj-del-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Private Project");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.DeleteAsync($"/api/projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
