using Tasker.Application.Commands;
using Tasker.Application.Commands.Handlers;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Entities;
using Tasker.Domain.Enums;

namespace Tasker.Application.Tests.Commands.Handlers;

public class DeleteTaskItemCommandHandlerTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly DeleteTaskItemCommandHandler _handler;

    public DeleteTaskItemCommandHandlerTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _realtimeNotifier = Substitute.For<IRealtimeNotifier>();
        _handler = new DeleteTaskItemCommandHandler(_taskRepository, _realtimeNotifier);
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteTask_WhenTaskExists()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Task to Delete",
            Description = "This task will be deleted",
            Priority = Priority.Medium,
            Status = Status.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var command = new DeleteTaskItemCommand(taskId);
        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _taskRepository.Received(1).GetByIdAsync(taskId);
        await _taskRepository.Received(1).DeleteAsync(existingTask);
        await _taskRepository.Received(1).SaveChangesAsync();
        await _realtimeNotifier.Received(1).NotifyTaskDeletedAsync(taskId);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenTaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskItemCommand(taskId);
        _taskRepository.GetByIdAsync(taskId).Returns((TaskItem?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _handler.HandleAsync(command));

        exception.Message.ShouldBe($"Task with ID {taskId} not found");
        
        await _taskRepository.DidNotReceive().DeleteAsync(Arg.Any<TaskItem>());
        await _taskRepository.DidNotReceive().SaveChangesAsync();
        await _realtimeNotifier.DidNotReceive().NotifyTaskDeletedAsync(Arg.Any<Guid>());
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    public async Task HandleAsync_ShouldDeleteTask_RegardlessOfPriority(Priority priority)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = $"{priority} Priority Task",
            Priority = priority,
            Status = Status.Pending
        };

        var command = new DeleteTaskItemCommand(taskId);
        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _taskRepository.Received(1).DeleteAsync(existingTask);
        await _taskRepository.Received(1).SaveChangesAsync();
        await _realtimeNotifier.Received(1).NotifyTaskDeletedAsync(taskId);
    }

    [Theory]
    [InlineData(Status.Pending)]
    [InlineData(Status.InProgress)]
    [InlineData(Status.Completed)]
    [InlineData(Status.Archived)]
    public async Task HandleAsync_ShouldDeleteTask_RegardlessOfStatus(Status status)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = $"{status} Task",
            Priority = Priority.Medium,
            Status = status
        };

        var command = new DeleteTaskItemCommand(taskId);
        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _taskRepository.Received(1).DeleteAsync(existingTask);
        await _taskRepository.Received(1).SaveChangesAsync();
        await _realtimeNotifier.Received(1).NotifyTaskDeletedAsync(taskId);
    }
}