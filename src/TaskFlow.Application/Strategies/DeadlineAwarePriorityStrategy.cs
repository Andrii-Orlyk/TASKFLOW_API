using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Strategies;

public sealed class DeadlineAwarePriorityStrategy : ITaskPriorityStrategy
{
    public TaskPriority Resolve(TaskPriority requestedPriority, DateTime? dueDate)
    {
        if (dueDate is null)
        {
            return requestedPriority;
        }

        var dueDateValue = dueDate.Value.Date;
        var today = DateTime.UtcNow.Date;
        var isDueSoon = dueDateValue == today || dueDateValue == today.AddDays(1);

        if (isDueSoon && requestedPriority < TaskPriority.High)
        {
            return TaskPriority.High;
        }

        return requestedPriority;
    }
}
