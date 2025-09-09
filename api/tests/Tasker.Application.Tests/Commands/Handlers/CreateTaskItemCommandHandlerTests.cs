using Tasker.Application.Commands;
using Tasker.Application.Commands.Handlers;
using Tasker.Application.EventHandlers;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Entities;
using Tasker.Domain.Enums;
using Tasker.Domain.Events;

namespace Tasker.Application.Tests.Commands.Handlers;

public class CreateTaskItemCommandHandlerTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IHighPriorityTaskChangedHandler _eventHandler;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly CreateTaskItemCommandHandler _handler;

    public CreateTaskItemCommandHandlerTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _eventHandler = Substitute.For<IHighPriorityTaskChangedHandler>();
        _realtimeNotifier = Substitute.For<IRealtimeNotifier>();
        _handler = new CreateTaskItemCommandHandler(_taskRepository, _eventHandler, _realtimeNotifier);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateTask_WhenValidCommandProvided()
    {
        // Arrange
        var command = new CreateTaskItemCommand(
            "Test Task",
            "Test Description",
            Priority.Medium,
            DateTime.UtcNow.AddDays(7));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(command.Id);
        result.Title.ShouldBe(command.Title);
        result.Description.ShouldBe(command.Description);
        result.Priority.ShouldBe(command.Priority);
        result.DueDate.ShouldBe(command.DueDate);
        result.Status.ShouldBe(Status.Pending);

        await _taskRepository.Received(1).AddAsync(Arg.Is<TaskItem>(t => 
            t.Id == command.Id &&
            t.Title == command.Title &&
            t.Description == command.Description &&
            t.Priority == command.Priority &&
            t.DueDate == command.DueDate));
        
        await _taskRepository.Received(1).SaveChangesAsync();
        await _realtimeNotifier.Received(1).NotifyTaskCreatedAsync(command.Id, command.Title);
    }

    [Fact]
    public async Task HandleAsync_ShouldTriggerHighPriorityEvent_WhenHighPriorityTask()
    {
        // Arrange
        var command = new CreateTaskItemCommand(
            "High Priority Task",
            "Urgent task",
            Priority.High,
            DateTime.UtcNow.AddHours(1));

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventHandler.Received(1).HandleAsync(Arg.Is<HighPriorityTaskChanged>(e =>
            e.TaskId == command.Id &&
            e.Title == command.Title &&
            e.Priority == Priority.High &&
            e.Reason == "Task created with high priority"));
    }

    [Fact]
    public async Task HandleAsync_ShouldNotTriggerHighPriorityEvent_WhenLowPriorityTask()
    {
        // Arrange
        var command = new CreateTaskItemCommand(
            "Low Priority Task",
            "Not urgent",
            Priority.Low,
            DateTime.UtcNow.AddDays(30));

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventHandler.DidNotReceive().HandleAsync(Arg.Any<HighPriorityTaskChanged>());
    }

    [Fact]
    public async Task HandleAsync_ShouldNotTriggerHighPriorityEvent_WhenMediumPriorityTask()
    {
        // Arrange
        var command = new CreateTaskItemCommand(
            "Medium Priority Task",
            "Moderately important",
            Priority.Medium,
            DateTime.UtcNow.AddDays(7));

        // Act
        await _handler.HandleAsync(command);

        // Assert
        await _eventHandler.DidNotReceive().HandleAsync(Arg.Any<HighPriorityTaskChanged>());
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateTaskWithNullDescription_WhenDescriptionNotProvided()
    {
        // Arrange
        var command = new CreateTaskItemCommand(
            "Task Without Description",
            null,
            Priority.Medium,
            null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Description.ShouldBeNull();
        result.DueDate.ShouldBeNull();

        await _taskRepository.Received(1).AddAsync(Arg.Is<TaskItem>(t => 
            t.Description == null && 
            t.DueDate == null));
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    public async Task HandleAsync_ShouldHandleAllPriorityLevels(Priority priority)
    {
        // Arrange
        var command = new CreateTaskItemCommand(
            $"{priority} Priority Task",
            "Test Description",
            priority,
            DateTime.UtcNow.AddDays(1));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Priority.ShouldBe(priority);

        if (priority == Priority.High)
        {
            await _eventHandler.Received(1).HandleAsync(Arg.Any<HighPriorityTaskChanged>());
        }
        else
        {
            await _eventHandler.DidNotReceive().HandleAsync(Arg.Any<HighPriorityTaskChanged>());
        }
    }
}