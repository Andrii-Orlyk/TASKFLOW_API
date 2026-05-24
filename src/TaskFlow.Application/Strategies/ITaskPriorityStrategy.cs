using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Strategies;

public interface ITaskPriorityStrategy
{
    TaskPriority Resolve(TaskPriority requestedPriority, DateTime? dueDate);
}
