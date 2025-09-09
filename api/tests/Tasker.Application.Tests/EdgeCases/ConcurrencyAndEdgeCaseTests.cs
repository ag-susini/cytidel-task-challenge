using Tasker.Application.Commands;
using Tasker.Application.Commands.Handlers;
using Tasker.Application.EventHandlers;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Entities;
using Tasker.Domain.Enums;
using Tasker.Domain.Events;

namespace Tasker.Application.Tests.EdgeCases;

public class ConcurrencyAndEdgeCaseTests
{
    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly IHighPriorityTaskChangedHandler _eventHandler = Substitute.For<IHighPriorityTaskChangedHandler>();
    private readonly IRealtimeNotifier _realtimeNotifier = Substitute.For<IRealtimeNotifier>();

    [Fact]
    public async Task UpdateHandler_ShouldOverwritePreviousValues_WhenTaskIsUpdated()
    {
        // This test verifies that updates properly overwrite existing values
        // including going from high priority back to low priority (which should not trigger events)
        
        // Arrange
        var taskId = Guid.NewGuid();
        var originalTask = new TaskItem
        {
            Id = taskId,
            Title = "Original Title",
            Description = "Original Description",
            Priority = Priority.High, // Starting as high priority
            Status = Status.InProgress,
            DueDate = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var command = new UpdateTaskItemCommand(
            taskId,
            "New Title",
            "New Description",
            Priority.Low, // Reducing priority to low
            Status.Completed,
            DateTime.UtcNow.AddDays(10));

        var handler = new UpdateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);
        _taskRepository.GetByIdAsync(taskId).Returns(originalTask);

        // Act
        await handler.HandleAsync(command);

        // Assert
        await _taskRepository.Received(1).UpdateAsync(originalTask);
        await _taskRepository.Received(1).SaveChangesAsync();
        
        // Verify all values were updated
        originalTask.Title.ShouldBe("New Title");
        originalTask.Description.ShouldBe("New Description");
        originalTask.Priority.ShouldBe(Priority.Low);
        originalTask.Status.ShouldBe(Status.Completed);
        originalTask.DueDate.ShouldBe(command.DueDate);
        originalTask.UpdatedAt.ShouldNotBeNull();
        originalTask.UpdatedAt.Value.ShouldBeGreaterThan(originalTask.CreatedAt);
        
        // Should not trigger high priority event when reducing priority
        await _eventHandler.DidNotReceive().HandleAsync(Arg.Any<HighPriorityTaskChanged>());
    }

    [Fact]
    public async Task CreateHandler_ShouldHandleRapidDuplicateCreation()
    {
        // Test rapid creation of tasks with the same title to ensure no race conditions
        
        // Arrange
        var tasks = new List<Task>();
        var handler = new CreateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);
        
        // Act - Create 10 tasks in parallel with same title
        for (int i = 0; i < 10; i++)
        {
            var command = new CreateTaskItemCommand(
                "Duplicate Title",
                "Description",
                Priority.Medium,
                DateTime.UtcNow);
            
            tasks.Add(Task.Run(async () => await handler.HandleAsync(command)));
        }
        
        await Task.WhenAll(tasks);

        // Assert - All tasks should be created successfully
        await _taskRepository.Received(10).AddAsync(Arg.Any<TaskItem>());
        await _taskRepository.Received(10).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateHandler_ShouldHandleNullUpdateScenarios()
    {
        // Test updating a task with null values to ensure proper handling
        
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Existing Title",
            Description = "Existing Description",
            Priority = Priority.High,
            Status = Status.InProgress,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var command = new UpdateTaskItemCommand(
            taskId,
            "New Title",
            null, // Null description
            Priority.Low,
            Status.Completed,
            null); // Null due date

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);
        var handler = new UpdateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);

        // Act
        await handler.HandleAsync(command);

        // Assert
        existingTask.Description.ShouldBeNull();
        existingTask.DueDate.ShouldBeNull();
        await _taskRepository.Received(1).UpdateAsync(existingTask);
    }

    [Fact]
    public async Task DeleteHandler_ShouldHandleDoubleDelete_Gracefully()
    {
        // Test deleting a task that's already been deleted
        
        // Arrange
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskItemCommand(taskId);
        var handler = new DeleteTaskItemCommandHandler(_taskRepository, _realtimeNotifier);
        
        _taskRepository.GetByIdAsync(taskId).Returns((TaskItem?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));
        
        exception.Message.ShouldContain("not found");
        await _taskRepository.DidNotReceive().DeleteAsync(Arg.Any<TaskItem>());
    }

    [Fact]
    public async Task HighPriorityHandler_ShouldHandleRapidPriorityChanges()
    {
        // Test rapid priority changes to ensure events are properly handled
        
        // Arrange
        var taskId = Guid.NewGuid();
        var handler = new UpdateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);
        
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Priority = Priority.Low,
            Status = Status.Pending
        };

        _taskRepository.GetByIdAsync(taskId).Returns(task);

        // Act - Rapidly change priority multiple times
        var commands = new[]
        {
            new UpdateTaskItemCommand(taskId, "Task", null, Priority.High, Status.Pending, null),
            new UpdateTaskItemCommand(taskId, "Task", null, Priority.Low, Status.Pending, null),
            new UpdateTaskItemCommand(taskId, "Task", null, Priority.High, Status.Pending, null)
        };

        foreach (var cmd in commands)
        {
            await handler.HandleAsync(cmd);
            task.Priority = cmd.Priority; // Simulate the update
        }

        // Assert - Should have triggered high priority event twice (first and third update)
        await _eventHandler.Received(2).HandleAsync(Arg.Any<HighPriorityTaskChanged>());
    }

    [Fact]
    public async Task CreateHandler_ShouldHandleMaxLengthBoundaries()
    {
        // Test creating tasks with maximum allowed field lengths
        
        // Arrange
        var maxTitle = new string('A', 200); // Assuming max is 200
        var maxDescription = new string('B', 2000); // Assuming max is 2000
        
        var command = new CreateTaskItemCommand(
            maxTitle,
            maxDescription,
            Priority.Medium,
            DateTime.MaxValue.AddDays(-1)); // Near max date

        var handler = new CreateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.Title.Length.ShouldBe(200);
        result.Description?.Length.ShouldBe(2000);
        await _taskRepository.Received(1).AddAsync(Arg.Is<TaskItem>(t => 
            t.Title.Length == 200 && 
            t.Description!.Length == 2000));
    }

    [Fact]
    public async Task UpdateHandler_ShouldMaintainInvariantsWhenTransitioningStatus()
    {
        // Test that certain invariants are maintained when transitioning between statuses
        
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Task",
            Priority = Priority.High,
            Status = Status.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Try to move a completed task back to pending
        var command = new UpdateTaskItemCommand(
            taskId,
            "Task",
            null,
            Priority.High,
            Status.Pending,
            DateTime.UtcNow);

        _taskRepository.GetByIdAsync(taskId).Returns(existingTask);
        var handler = new UpdateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);

        // Act
        await handler.HandleAsync(command);

        // Assert - Status change should be allowed (no business rule preventing it in current implementation)
        existingTask.Status.ShouldBe(Status.Pending);
        existingTask.UpdatedAt.ShouldNotBeNull();
        existingTask.UpdatedAt.Value.ShouldBeGreaterThan(existingTask.CreatedAt);
    }

    [Fact]
    public async Task EventHandler_ShouldRecordCriticalEvent_EvenWhenNotificationFails()
    {
        // This test verifies current behavior: if notification fails, the whole operation fails
        // This reveals a potential improvement: we might want to record critical events even if notifications fail
        
        // Arrange
        var criticalEventSink = Substitute.For<ICriticalEventSink>();
        var realtimeNotifier = Substitute.For<IRealtimeNotifier>();
        
        // Simulate notification failure after critical event is recorded
        realtimeNotifier.NotifyHighPriorityTaskChangedAsync(
            Arg.Any<Guid>(), 
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("Network error")));

        var handler = new HighPriorityTaskChangedHandler(criticalEventSink, realtimeNotifier);
        var domainEvent = new HighPriorityTaskChanged(
            Guid.NewGuid(),
            "Task",
            Priority.High,
            "Test",
            DateTime.UtcNow);

        // Act & Assert - Currently throws due to notification failure
        var exception = await Should.ThrowAsync<Exception>(
            async () => await handler.HandleAsync(domainEvent));
        
        exception.Message.ShouldBe("Network error");
        
        // Critical event should be recorded before notification attempt
        await criticalEventSink.Received(1).RecordAsync(domainEvent, default);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task Handler_ShouldHandleExtremeNumericValues(int dayOffset)
    {
        // Test handling of extreme date values
        
        // Arrange
        DateTime? dueDate = null;
        try
        {
            dueDate = DateTime.UtcNow.AddDays(dayOffset);
        }
        catch
        {
            // If date calculation throws, use null
            dueDate = null;
        }

        var command = new CreateTaskItemCommand(
            "Test Task",
            "Description",
            Priority.Medium,
            dueDate);

        var handler = new CreateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.DueDate.ShouldBe(dueDate);
    }
}