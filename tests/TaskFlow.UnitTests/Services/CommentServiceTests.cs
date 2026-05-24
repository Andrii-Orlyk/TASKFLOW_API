using FluentAssertions;
using Moq;
using TaskFlow.Application.DTOs.Comments;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.UnitTests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepository = new();
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly CommentService _commentService;
    private readonly Guid _userId = Guid.NewGuid();

    public CommentServiceTests()
    {
        _currentUserService.Setup(service => service.IsAuthenticated).Returns(true);
        _currentUserService.Setup(service => service.UserId).Returns(_userId);

        _commentService = new CommentService(
            _commentRepository.Object,
            _taskRepository.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task CreateComment_ShouldFail_WhenContentIsEmpty()
    {
        var result = await _commentService.CreateAsync(
            Guid.NewGuid(),
            new CreateCommentRequest("   "),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("comment.empty_content");
    }

    [Fact]
    public async Task CreateComment_ShouldFail_WhenTaskBelongsToAnotherUser()
    {
        var taskId = Guid.NewGuid();
        _taskRepository
            .Setup(repository => repository.GetByIdForUserAsync(taskId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var result = await _commentService.CreateAsync(
            taskId,
            new CreateCommentRequest("Valid comment"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("task.not_found");
    }

    [Fact]
    public async Task CreateComment_ShouldSucceed_WhenTaskBelongsToCurrentUser()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            ProjectId = Guid.NewGuid(),
            Title = "Task",
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskRepository
            .Setup(repository => repository.GetByIdForUserAsync(taskId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var result = await _commentService.CreateAsync(
            taskId,
            new CreateCommentRequest("Valid comment"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Content.Should().Be("Valid comment");
        _commentRepository.Verify(
            repository => repository.AddAsync(It.Is<TaskComment>(comment =>
                comment.TaskItemId == taskId && comment.AuthorId == _userId), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
