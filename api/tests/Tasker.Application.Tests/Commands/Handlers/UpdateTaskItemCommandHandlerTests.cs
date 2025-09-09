using Tasker.Application.Commands;
using Tasker.Application.Commands.Handlers;
using Tasker.Application.EventHandlers;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Entities;
using Tasker.Domain.Enums;
using Tasker.Domain.Events;

namespace Tasker.Application.Tests.Commands.Handlers;

public class UpdateTaskItemCommandHandlerTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IHighPriorityTaskChangedHandler _eventHandler;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly UpdateTaskItemCommandHandler _handler;

    public UpdateTaskItemCommandHandlerTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _eventHandler = Substitute.For<IHighPriorityTaskChangedHandler>();
        _realtimeNotifier = Substitute.For<IRealtimeNotifier>();
        _handler = new UpdateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateTask_WhenValidCommandProvided()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Old Title",
            Description = "Old Description",
            Priority = Priority.Low,
            Status = Status.Pending,
            DueDate = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var command = new UpdateTaskItemCommand(
            taskId,
            "New Title",
            "New Description",
            Priority.Medium,
            Status.InProgress,
            DateTime.UtcNow.AddDays(10));

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingTask.Title.ShouldBe("New Title");
        existingTask.Description.ShouldBe("New Description");
        existingTask.Priority.ShouldBe(Priority.Medium);
        existingTask.Status.ShouldBe(Status.InProgress);
        existingTask.DueDate.ShouldBe(command.DueDate);
        existingTask.UpdatedAt.ShouldNotBeNull();
        existingTask.UpdatedAt.Value.ShouldBeGreaterThan(existingTask.CreatedAt);

        await _taskRepository.Received(1).UpdateAsync(existingTask);
        await _taskRepository.Received(1).SaveChangesAsync();
        await _realtimeNotifier.Received(1).NotifyTaskUpdatedAsync(taskId, "New Title");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenTaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var command = new UpdateTaskItemCommand(
            taskId,
            "Title",
            "Description",
            Priority.Medium,
            Status.Pending,
            DateTime.UtcNow);

        _taskRepository.GetByIdAsync(taskId).Returns((TaskItem?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _handler.HandleAsync(command));
        
        exception.Message.ShouldBe($"Task with ID {taskId} not found");
        
        await _taskRepository.DidNotReceive().UpdateAsync(Arg.Any<TaskItem>());
        await _taskRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_ShouldTriggerHighPriorityEvent_WhenPriorityElevatedToHigh()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Priority = Priority.Medium,
            Status = Status.Pending
        };

        var command = new UpdateTaskItemCommand(
            taskId,
            "Updated Task",
            "Description",
            Priority.High,
            Status.InProgress,
            DateTime.UtcNow);

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventHandler.Received(1).HandleAsync(Arg.Is<HighPriorityTaskChanged>(e =>
            e.TaskId == taskId &&
            e.Title == "Updated Task" &&
            e.Priority == Priority.High &&
            e.Reason == "Task priority elevated to High"));
    }

    [Fact]
    public async Task HandleAsync_ShouldTriggerHighPriorityEvent_WhenHighPriorityTaskUpdated()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "High Priority Task",
            Priority = Priority.High,
            Status = Status.InProgress
        };

        var command = new UpdateTaskItemCommand(
            taskId,
            "Updated High Priority Task",
            "Updated Description",
            Priority.High,
            Status.InProgress,
            DateTime.UtcNow);

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventHandler.Received(1).HandleAsync(Arg.Is<HighPriorityTaskChanged>(e =>
            e.TaskId == taskId &&
            e.Title == "Updated High Priority Task" &&
            e.Priority == Priority.High &&
            e.Reason == "High priority task updated"));
    }

    [Fact]
    public async Task HandleAsync_ShouldNotTriggerHighPriorityEvent_WhenPriorityReducedFromHigh()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "High Priority Task",
            Priority = Priority.High,
            Status = Status.InProgress
        };

        var command = new UpdateTaskItemCommand(
            taskId,
            "Reduced Priority Task",
            "Description",
            Priority.Medium,
            Status.InProgress,
            DateTime.UtcNow);

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventHandler.DidNotReceive().HandleAsync(Arg.Any<HighPriorityTaskChanged>());
    }

    [Fact]
    public async Task HandleAsync_ShouldNotTriggerHighPriorityEvent_WhenUpdatingLowPriorityTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Low Priority Task",
            Priority = Priority.Low,
            Status = Status.Pending
        };

        var command = new UpdateTaskItemCommand(
            taskId,
            "Updated Low Priority Task",
            "Description",
            Priority.Medium,
            Status.InProgress,
            DateTime.UtcNow);

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventHandler.DidNotReceive().HandleAsync(Arg.Any<HighPriorityTaskChanged>());
    }

    [Theory]
    [InlineData(Priority.Low, Priority.High, "Task priority elevated to High")]
    [InlineData(Priority.Medium, Priority.High, "Task priority elevated to High")]
    [InlineData(Priority.High, Priority.High, "High priority task updated")]
    public async Task HandleAsync_ShouldGenerateCorrectReason_ForHighPriorityChanges(
        Priority originalPriority, 
        Priority newPriority, 
        string expectedReason)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Priority = originalPriority,
            Status = Status.Pending
        };

        var command = new UpdateTaskItemCommand(
            taskId,
            "Updated Task",
            "Description",
            newPriority,
            Status.InProgress,
            DateTime.UtcNow);

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventHandler.Received(1).HandleAsync(Arg.Is<HighPriorityTaskChanged>(e =>
            e.Reason == expectedReason));
    }
}