using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Policies;

public interface ITaskStatusPolicy
{
    bool CanTransition(TaskItemStatus from, TaskItemStatus to);
}
