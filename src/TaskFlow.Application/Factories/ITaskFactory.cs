using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Factories;

public interface ITaskFactory
{
    TaskItem Create(
        Guid projectId,
        string title,
        string? description,
        TaskPriority priority,
        DateTime? dueDate);
}
