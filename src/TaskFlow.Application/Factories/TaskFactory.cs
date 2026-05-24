using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Factories;

public sealed class TaskFactory : ITaskFactory
{
    public TaskItem Create(
        Guid projectId,
        string title,
        string? description,
        TaskPriority priority,
        DateTime? dueDate)
    {
        var utcNow = DateTime.UtcNow;
        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title.Trim(),
            Description = normalizedDescription,
            Status = TaskItemStatus.Todo,
            Priority = priority,
            DueDate = dueDate,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
}
