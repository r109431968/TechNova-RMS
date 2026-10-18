using Microsoft.Extensions.DependencyInjection;

namespace Rms.Application;

public static class ApplicationServiceCollectionExtensions
{
    // Lifetimes: services are stateless -> Transient. Repositories/notifier (registered in
    // Infrastructure) hold shared state -> Singleton. Swap the strategy here to change matching.
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new ReportSettings());
        services.AddSingleton<IMatchScoringStrategy, SkillCoverageScoring>();

        services.AddTransient<EmployeeService>();
        services.AddTransient<ProjectService>();
        services.AddTransient<AllocationService>();
        services.AddTransient<MatchingService>();
        services.AddTransient<ReportService>();
        return services;
    }
}
