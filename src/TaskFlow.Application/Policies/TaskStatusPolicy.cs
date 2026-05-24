using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Policies;

public sealed class TaskStatusPolicy : ITaskStatusPolicy
{
    private static readonly Dictionary<TaskItemStatus, HashSet<TaskItemStatus>> AllowedTransitions =
        new()
        {
            [TaskItemStatus.Todo] = [TaskItemStatus.InProgress, TaskItemStatus.Cancelled],
            [TaskItemStatus.InProgress] = [TaskItemStatus.Done, TaskItemStatus.Cancelled],
            [TaskItemStatus.Done] = [TaskItemStatus.InProgress],
            [TaskItemStatus.Cancelled] = [TaskItemStatus.Todo]
        };

    public bool CanTransition(TaskItemStatus from, TaskItemStatus to) =>
        AllowedTransitions.TryGetValue(from, out var allowedTargets) && allowedTargets.Contains(to);
}
