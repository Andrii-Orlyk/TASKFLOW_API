using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Api.Models;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Comments;

public sealed class CommentsIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateComment_ShouldSucceed_WhenTaskBelongsToCurrentUser()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("comment-author@example.com");
        client.UseBearerToken(token);

        var projectId = await client.CreateProjectAsync("Project");
        var taskId = await client.CreateTaskAsync(projectId, "Commented task");

        var comment = await client.CreateCommentAsync(taskId, "Looks good.");

        comment.TaskItemId.Should().Be(taskId);
        comment.Content.Should().Be("Looks good.");
        comment.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteComment_ShouldReturn404_WhenCommentBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("comment-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("comment-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Owner Project");
        var taskId = await ownerClient.CreateTaskAsync(projectId, "Owner task");
        var comment = await ownerClient.CreateCommentAsync(taskId, "Private note");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.DeleteAsync($"/api/comments/{comment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("comment.not_found");
    }

    [Fact]
    public async Task CreateComment_ShouldReturn404_WhenTaskBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("comment-task-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("comment-task-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Owner Project");
        var taskId = await ownerClient.CreateTaskAsync(projectId, "Owner task");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.PostAsJsonAsync(
            $"/api/tasks/{taskId}/comments",
            new { content = "Unauthorized comment" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("task.not_found");
    }
}
