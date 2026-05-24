using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Comments;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Enums;

namespace TaskFlow.IntegrationTests.Infrastructure;

internal static class IntegrationTestClientExtensions
{
    public static async Task<string> RegisterUserAsync(
        this HttpClient client,
        string email,
        string password = "Password123!")
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, "Test", "User"));

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }

    public static async Task<string> LoginUserAsync(
        this HttpClient client,
        string email,
        string password = "Password123!")
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }

    public static void UseBearerToken(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public static async Task<Guid> CreateProjectAsync(this HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/projects", new CreateProjectRequest(name, null));
        response.EnsureSuccessStatusCode();

        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        return project!.Id;
    }

    public static async Task<CommentResponse> CreateCommentAsync(
        this HttpClient client,
        Guid taskId,
        string content)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/tasks/{taskId}/comments",
            new CreateCommentRequest(content));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<CommentResponse>())!;
    }

    public static async Task<Guid> CreateTaskAsync(
        this HttpClient client,
        Guid projectId,
        string title,
        TaskItemStatus? targetStatus = null,
        DateTime? dueDate = null)
    {
        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest(projectId, title, null, TaskPriority.Medium, dueDate));

        response.EnsureSuccessStatusCode();

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(TestJsonOptions.Default);

        if (targetStatus is null or TaskItemStatus.Todo)
        {
            return task!.Id;
        }

        if (targetStatus == TaskItemStatus.InProgress)
        {
            (await client.UpdateTaskStatusAsync(task!.Id, TaskItemStatus.InProgress)).EnsureSuccessStatusCode();
            return task.Id;
        }

        if (targetStatus == TaskItemStatus.Done)
        {
            (await client.UpdateTaskStatusAsync(task!.Id, TaskItemStatus.InProgress)).EnsureSuccessStatusCode();
            (await client.UpdateTaskStatusAsync(task.Id, TaskItemStatus.Done)).EnsureSuccessStatusCode();
            return task.Id;
        }

        (await client.UpdateTaskStatusAsync(task!.Id, targetStatus.Value)).EnsureSuccessStatusCode();
        return task.Id;
    }

    public static Task<HttpResponseMessage> UpdateTaskStatusAsync(
        this HttpClient client,
        Guid taskId,
        TaskItemStatus status) =>
        client.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/status",
            new UpdateTaskStatusRequest(status));
}
