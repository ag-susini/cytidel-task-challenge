using Microsoft.Extensions.DependencyInjection;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Shared.Commands;

internal sealed class InMemoryCommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task DispatchAsync<TCommand>(TCommand command) where TCommand : class, ICommand
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.HandleAsync(command);
    }

    public async Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command)
    {
        using var scope = serviceProvider.CreateScope();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.HandleAsync))
                     ?? throw new InvalidOperationException($"Method {nameof(ICommandHandler<ICommand<TResult>, TResult>.HandleAsync)} not found on handler type {handlerType}");
        
        var result = method.Invoke(handler, [command])
                     ?? throw new InvalidOperationException("Handler returned null");
            
        return await (Task<TResult>)result;
    }
}