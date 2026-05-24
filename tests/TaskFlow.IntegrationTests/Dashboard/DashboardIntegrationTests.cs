using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Dashboard;

public sealed class DashboardIntegrationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Dashboard_ShouldCountOnlyCurrentUserData()
    {
        var clientA = CreateClient();
        var clientB = CreateClient();

        var tokenA = await clientA.RegisterUserAsync("dashboard-a@example.com");
        var tokenB = await clientB.RegisterUserAsync("dashboard-b@example.com");

        clientA.UseBearerToken(tokenA);
        clientB.UseBearerToken(tokenB);

        var projectA1 = await clientA.CreateProjectAsync("A Project 1");
        await clientA.CreateProjectAsync("A Project 2");

        await clientA.CreateTaskAsync(projectA1, "Todo task", TaskItemStatus.Todo);
        await clientA.CreateTaskAsync(projectA1, "In progress task", TaskItemStatus.InProgress);
        await clientA.CreateTaskAsync(projectA1, "Done task", TaskItemStatus.Done);

        var projectB = await clientB.CreateProjectAsync("B Project");
        await clientB.CreateTaskAsync(projectB, "B task", TaskItemStatus.Todo);

        var response = await clientA.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>();
        summary.Should().BeEquivalentTo(new DashboardSummaryResponse(
            TotalProjects: 2,
            TotalTasks: 3,
            TodoTasks: 1,
            InProgressTasks: 1,
            DoneTasks: 1,
            OverdueTasks: 0));
    }

    [Fact]
    public async Task Dashboard_ShouldCountTodoInProgressDoneAndOverdueTasks()
    {
        var client = CreateClient();
        var token = await client.RegisterUserAsync("dashboard-counts@example.com");
        client.UseBearerToken(token);

        var projectId = await client.CreateProjectAsync("Dashboard Project");

        await client.CreateTaskAsync(projectId, "Todo task", TaskItemStatus.Todo);
        await client.CreateTaskAsync(projectId, "In progress task", TaskItemStatus.InProgress);
        await client.CreateTaskAsync(projectId, "Done task", TaskItemStatus.Done);
        await Factory.SeedOverdueTaskAsync(projectId, "Overdue task");

        var response = await client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>();
        summary.Should().BeEquivalentTo(new DashboardSummaryResponse(
            TotalProjects: 1,
            TotalTasks: 4,
            TodoTasks: 2,
            InProgressTasks: 1,
            DoneTasks: 1,
            OverdueTasks: 1));
    }
}
