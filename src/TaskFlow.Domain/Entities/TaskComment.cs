using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class TaskComment : BaseEntity
{
    public Guid TaskItemId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;

    public TaskItem TaskItem { get; set; } = null!;
    public User Author { get; set; } = null!;
}
