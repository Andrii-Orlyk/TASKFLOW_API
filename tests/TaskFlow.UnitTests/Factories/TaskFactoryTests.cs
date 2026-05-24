using FluentAssertions;
using TaskFlow.Domain.Enums;
using DomainTaskFactory = TaskFlow.Application.Factories.TaskFactory;

namespace TaskFlow.UnitTests.Factories;

public class TaskFactoryTests
{
    private readonly DomainTaskFactory _factory = new();

    [Fact]
    public void TaskFactory_ShouldCreateTaskWithDefaultTodoStatus()
    {
        var task = _factory.Create(
            Guid.NewGuid(),
            "  My task  ",
            "   ",
            TaskPriority.Medium,
            DateTime.UtcNow.Date.AddDays(3));

        task.Status.Should().Be(TaskItemStatus.Todo);
        task.Title.Should().Be("My task");
        task.Description.Should().BeNull();
        task.Priority.Should().Be(TaskPriority.Medium);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        task.UpdatedAt.Should().Be(task.CreatedAt);
        task.Id.Should().NotBe(Guid.Empty);
    }
}
