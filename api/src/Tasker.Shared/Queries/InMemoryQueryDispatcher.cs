using Microsoft.Extensions.DependencyInjection;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Shared.Queries;

internal sealed class InMemoryQueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
    {
        using var scope = serviceProvider.CreateScope();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync))
            ?? throw new InvalidOperationException($"Method {nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync)} not found on handler type {handlerType}");
        
        var result = method.Invoke(handler, [query])
            ?? throw new InvalidOperationException("Handler returned null");
            
        return await (Task<TResult>)result;
    }
}