using FluentAssertions;
using TaskFlow.Application.Strategies;
using TaskFlow.Domain.Enums;

namespace TaskFlow.UnitTests.Strategies;

public class DeadlineAwarePriorityStrategyTests
{
    private readonly DeadlineAwarePriorityStrategy _strategy = new();

    [Fact]
    public void PriorityStrategy_ShouldIncreasePriority_WhenDeadlineIsSoon()
    {
        var dueDate = DateTime.UtcNow.Date.AddDays(1);

        var priority = _strategy.Resolve(TaskPriority.Low, dueDate);

        priority.Should().Be(TaskPriority.High);
    }

    [Theory]
    [InlineData(TaskPriority.High)]
    [InlineData(TaskPriority.Medium)]
    public void Resolve_ShouldKeepPriority_WhenDueDateIsNotSoon(TaskPriority requestedPriority)
    {
        var dueDate = DateTime.UtcNow.Date.AddDays(5);

        var priority = _strategy.Resolve(requestedPriority, dueDate);

        priority.Should().Be(requestedPriority);
    }

    [Fact]
    public void Resolve_ShouldKeepHighPriority_WhenDeadlineIsSoon()
    {
        var dueDate = DateTime.UtcNow.Date;

        var priority = _strategy.Resolve(TaskPriority.High, dueDate);

        priority.Should().Be(TaskPriority.High);
    }
}
