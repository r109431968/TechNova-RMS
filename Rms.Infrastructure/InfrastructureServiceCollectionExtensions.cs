using Microsoft.Extensions.DependencyInjection;
using Rms.Infrastructure.Notifications;
using Rms.Infrastructure.Persistence;
using Rms.Infrastructure.Seeding;

namespace Rms.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    // Singletons: each repository owns the in-memory state and its JSON file, so there must be exactly one.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dataDirectory)
    {
        services.AddSingleton<IEmployeeRepository>(_ =>
            new JsonEmployeeRepository(Path.Combine(dataDirectory, "employees.json")));
        services.AddSingleton<IProjectRepository>(_ =>
            new JsonProjectRepository(Path.Combine(dataDirectory, "projects.json")));
        services.AddSingleton<IAllocationRepository>(_ =>
            new JsonAllocationRepository(Path.Combine(dataDirectory, "allocations.json")));

        services.AddSingleton<INotifier, ConsoleNotifier>();
        services.AddSingleton<DataSeeder>();
        return services;
    }
}
