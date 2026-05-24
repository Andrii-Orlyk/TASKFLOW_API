using FluentAssertions;
using TaskFlow.Application.Policies;
using TaskFlow.Domain.Enums;

namespace TaskFlow.UnitTests.Policies;

public class TaskStatusPolicyTests
{
    private readonly TaskStatusPolicy _policy = new();

    [Theory]
    [InlineData(TaskItemStatus.Todo, TaskItemStatus.InProgress, true)]
    [InlineData(TaskItemStatus.Todo, TaskItemStatus.Cancelled, true)]
    [InlineData(TaskItemStatus.InProgress, TaskItemStatus.Done, true)]
    [InlineData(TaskItemStatus.Done, TaskItemStatus.InProgress, true)]
    [InlineData(TaskItemStatus.Cancelled, TaskItemStatus.Todo, true)]
    public void CanTransition_ShouldAllowValidTransitions(
        TaskItemStatus from,
        TaskItemStatus to,
        bool expected)
    {
        _policy.CanTransition(from, to).Should().Be(expected);
    }

    [Fact]
    public void TaskStatusPolicy_ShouldRejectInvalidTransition()
    {
        _policy.CanTransition(TaskItemStatus.Todo, TaskItemStatus.Done).Should().BeFalse();
    }
}
