using FluentAssertions;
using Moq;
using TaskFlow.Application.DTOs.Tasks;
using DomainTaskFactory = TaskFlow.Application.Factories.TaskFactory;
using TaskFlow.Application.Factories;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Policies;
using TaskFlow.Application.Services;
using TaskFlow.Application.Strategies;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.UnitTests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<IProjectRepository> _projectRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly ITaskFactory _taskFactory = new DomainTaskFactory();
    private readonly ITaskPriorityStrategy _priorityStrategy = new DeadlineAwarePriorityStrategy();
    private readonly ITaskStatusPolicy _statusPolicy = new TaskStatusPolicy();
    private readonly TaskService _taskService;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _currentUserService.Setup(service => service.IsAuthenticated).Returns(true);
        _currentUserService.Setup(service => service.UserId).Returns(_userId);

        _taskService = new TaskService(
            _taskRepository.Object,
            _projectRepository.Object,
            _currentUserService.Object,
            _taskFactory,
            _priorityStrategy,
            _statusPolicy);
    }

    [Fact]
    public async Task CreateTask_ShouldFail_WhenDueDateIsInPast()
    {
        var request = new CreateTaskRequest(
            Guid.NewGuid(),
            "Task",
            null,
            TaskPriority.Medium,
            DateTime.UtcNow.Date.AddDays(-1));

        var result = await _taskService.CreateAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("task.invalid_due_date");
    }

    [Fact]
    public async Task CreateTask_ShouldFail_WhenProjectDoesNotBelongToCurrentUser()
    {
        var projectId = Guid.NewGuid();
        var request = new CreateTaskRequest(
            projectId,
            "Task",
            null,
            TaskPriority.Medium,
            DateTime.UtcNow.Date.AddDays(2));

        _projectRepository
            .Setup(repository => repository.GetByIdForUserAsync(projectId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var result = await _taskService.CreateAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("project.not_found");
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldSetCompletedAt_WhenStatusIsDone()
    {
        var task = CreateTask(TaskItemStatus.InProgress);
        _taskRepository
            .Setup(repository => repository.GetByIdForUserAsync(task.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var result = await _taskService.UpdateStatusAsync(
            task.Id,
            new UpdateTaskStatusRequest(TaskItemStatus.Done),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskItemStatus.Done);
        task.CompletedAt.Should().NotBeNull();
        task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldClearCompletedAt_WhenStatusChangesFromDone()
    {
        var task = CreateTask(TaskItemStatus.Done);
        task.CompletedAt = DateTime.UtcNow.AddDays(-1);
        _taskRepository
            .Setup(repository => repository.GetByIdForUserAsync(task.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var result = await _taskService.UpdateStatusAsync(
            task.Id,
            new UpdateTaskStatusRequest(TaskItemStatus.InProgress),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskItemStatus.InProgress);
        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldFail_WhenTransitionIsInvalid()
    {
        var task = CreateTask(TaskItemStatus.Todo);
        _taskRepository
            .Setup(repository => repository.GetByIdForUserAsync(task.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var result = await _taskService.UpdateStatusAsync(
            task.Id,
            new UpdateTaskStatusRequest(TaskItemStatus.Done),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("task.invalid_status_transition");
    }

    private static TaskItem CreateTask(TaskItemStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Title = "Task",
            Status = status,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
}
