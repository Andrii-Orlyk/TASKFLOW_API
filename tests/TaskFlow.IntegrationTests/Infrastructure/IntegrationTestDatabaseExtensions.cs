using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.IntegrationTests.Infrastructure;

internal static class IntegrationTestDatabaseExtensions
{
    public static async Task SeedOverdueTaskAsync(
        this CustomWebApplicationFactory factory,
        Guid projectId,
        string title)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
        var now = DateTime.UtcNow;

        dbContext.TaskItems.Add(new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title,
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Medium,
            DueDate = now.AddDays(-3),
            CreatedAt = now,
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync();
    }
}
