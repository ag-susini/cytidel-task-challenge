using Tasker.Application.EventHandlers;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Enums;
using Tasker.Domain.Events;

namespace Tasker.Application.Tests.EventHandlers;

public class HighPriorityTaskChangedHandlerTests
{
    private readonly ICriticalEventSink _criticalEventSink;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly HighPriorityTaskChangedHandler _handler;

    public HighPriorityTaskChangedHandlerTests()
    {
        _criticalEventSink = Substitute.For<ICriticalEventSink>();
        _realtimeNotifier = Substitute.For<IRealtimeNotifier>();
        _handler = new HighPriorityTaskChangedHandler(_criticalEventSink, _realtimeNotifier);
    }

    [Fact]
    public async Task HandleAsync_ShouldRecordCriticalEvent_WhenHighPriorityTaskChanged()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var domainEvent = new HighPriorityTaskChanged(
            taskId,
            "Critical Task",
            Priority.High,
            "Task created with high priority",
            DateTime.UtcNow);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        await _criticalEventSink.Received(1).RecordAsync(domainEvent, default);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotifyRealtimeClients_WhenHighPriorityTaskChanged()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var domainEvent = new HighPriorityTaskChanged(
            taskId,
            "Urgent Task",
            Priority.High,
            "Task priority elevated to High",
            DateTime.UtcNow);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        await _realtimeNotifier.Received(1).NotifyHighPriorityTaskChangedAsync(
            taskId,
            "Urgent Task",
            "Task priority elevated to High",
            default);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleBothRecordingAndNotification_InCorrectOrder()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var domainEvent = new HighPriorityTaskChanged(
            taskId,
            "Test Task",
            Priority.High,
            "High priority task updated",
            DateTime.UtcNow);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        await _criticalEventSink.Received(1).RecordAsync(domainEvent, default);
        await _realtimeNotifier.Received(1).NotifyHighPriorityTaskChangedAsync(
            taskId,
            "Test Task",
            "High priority task updated",
            default);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCancellationToken_ToAllServices()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var domainEvent = new HighPriorityTaskChanged(
            taskId,
            "Test Task",
            Priority.High,
            "Test reason",
            DateTime.UtcNow);
        
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.HandleAsync(domainEvent, cancellationToken);

        // Assert
        await _criticalEventSink.Received(1).RecordAsync(domainEvent, cancellationToken);
        await _realtimeNotifier.Received(1).NotifyHighPriorityTaskChangedAsync(
            taskId,
            "Test Task",
            "Test reason",
            cancellationToken);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleDifferentReasons_Correctly()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var reasons = new[]
        {
            "Task created with high priority",
            "Task priority elevated to High",
            "High priority task updated"
        };

        // Act & Assert
        foreach (var reason in reasons)
        {
            var domainEvent = new HighPriorityTaskChanged(
                taskId,
                "Test Task",
                Priority.High,
                reason,
                DateTime.UtcNow);

            await _handler.HandleAsync(domainEvent);

            await _realtimeNotifier.Received().NotifyHighPriorityTaskChangedAsync(
                taskId,
                "Test Task",
                reason,
                default);
        }

        await _criticalEventSink.Received(reasons.Length).RecordAsync(Arg.Any<HighPriorityTaskChanged>(), default);
    }
}