using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Shared.Queries;

public static class Extentions
{
    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        var assembly = Assembly.GetCallingAssembly();
            
        services.AddSingleton<IQueryDispatcher, InMemoryQueryDispatcher>();
        services.Scan(s => s.FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
            
        return services;
    }
}