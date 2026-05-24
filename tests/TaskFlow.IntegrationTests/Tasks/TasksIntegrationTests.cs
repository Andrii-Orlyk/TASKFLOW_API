using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Api.Models;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Tasks;

public sealed class TasksIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateTask_ShouldReturn404_WhenProjectBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Owner Project");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest(projectId, "Foreign task", null, TaskPriority.Medium, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error.Should().NotBeNull();
        error!.StatusCode.Should().Be(404);
        error.Code.Should().Be("project.not_found");
        error.Message.Should().NotBeNullOrWhiteSpace();
        error.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTask_ShouldReturn404_WhenTaskBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("task-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("task-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Owner Project");
        var taskId = await ownerClient.CreateTaskAsync(projectId, "Private task");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.GetAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTask_ShouldReturn400_WhenDueDateIsPast()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("past-due@example.com");
        client.UseBearerToken(token);

        var projectId = await client.CreateProjectAsync("Project");

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest(
                projectId,
                "Past due task",
                null,
                TaskPriority.Medium,
                DateTime.UtcNow.AddDays(-1)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        // Validator fires before service; FluentValidation ValidDueDate rule returns validation.failed.
        error!.Code.Should().Be("validation.failed");
        error.Message.Should().NotBeNullOrEmpty();
        error.Errors.Should().ContainMatch("*Due date*");
    }

    [Fact]
    public async Task CreateTask_ShouldReturn400_WhenTitleIsEmpty()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("empty-title@example.com");
        client.UseBearerToken(token);

        var projectId = await client.CreateProjectAsync("Project");

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest(projectId, "", null, TaskPriority.Medium, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("validation.failed");
        error.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTask_ShouldAcceptStringEnumPriority()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("enum-priority@example.com");
        client.UseBearerToken(token);

        var projectId = await client.CreateProjectAsync("Project");

        // Send priority as a string value ("High") instead of integer.
        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new { projectId, title = "Enum test task", priority = "High", dueDate = (DateTime?)null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(TestJsonOptions.Default);
        task!.Priority.Should().Be(TaskPriority.High);
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldReturn409_WhenTransitionIsInvalid()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("invalid-transition@example.com");
        client.UseBearerToken(token);

        var projectId = await client.CreateProjectAsync("Project");
        var taskId = await client.CreateTaskAsync(projectId, "Todo task");

        var response = await client.UpdateTaskStatusAsync(taskId, TaskItemStatus.Done);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("task.invalid_status_transition");
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldAcceptStringEnumStatus()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("enum-status@example.com");
        client.UseBearerToken(token);

        var projectId = await client.CreateProjectAsync("Project");
        var taskId = await client.CreateTaskAsync(projectId, "Enum status task");

        // Patch using string "InProgress" instead of integer.
        var inProgressResponse = await client.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/status",
            new { status = "InProgress" });
        inProgressResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var doneResponse = await client.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/status",
            new { status = "Done" });
        doneResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var task = await doneResponse.Content.ReadFromJsonAsync<TaskResponse>(TestJsonOptions.Default);
        task!.Status.Should().Be(TaskItemStatus.Done);
        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldSetCompletedAt_WhenStatusIsDone()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("completed-at@example.com");
        client.UseBearerToken(token);

        var projectId = await client.CreateProjectAsync("Project");
        var taskId = await client.CreateTaskAsync(projectId, "Finish me", TaskItemStatus.Done);

        var response = await client.GetAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(TestJsonOptions.Default);
        task!.Status.Should().Be(TaskItemStatus.Done);
        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTask_ShouldReturn404_WhenTaskBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("task-upd-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("task-upd-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Owner Project");
        var taskId = await ownerClient.CreateTaskAsync(projectId, "Private task");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.PutAsJsonAsync(
            $"/api/tasks/{taskId}",
            new UpdateTaskRequest("Updated title", null, TaskPriority.Low, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("task.not_found");
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldReturn404_WhenTaskBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("task-patch-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("task-patch-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Owner Project");
        var taskId = await ownerClient.CreateTaskAsync(projectId, "Private task");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.UpdateTaskStatusAsync(taskId, TaskItemStatus.InProgress);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_ShouldReturn404_WhenTaskBelongsToAnotherUser()
    {
        var ownerClient = CreateClient();
        var otherClient = CreateClient();

        var ownerToken = await ownerClient.RegisterUserAsync("task-del-owner@example.com");
        var otherToken = await otherClient.RegisterUserAsync("task-del-other@example.com");

        ownerClient.UseBearerToken(ownerToken);
        var projectId = await ownerClient.CreateProjectAsync("Owner Project");
        var taskId = await ownerClient.CreateTaskAsync(projectId, "Private task");

        otherClient.UseBearerToken(otherToken);

        var response = await otherClient.DeleteAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
