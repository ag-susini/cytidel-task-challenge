namespace Tasker.Shared.Abstractions.Commands;

public interface ICommandDispatcher
{
    Task DispatchAsync<TCommand>(TCommand command) where TCommand : class, ICommand;
    Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command);
}