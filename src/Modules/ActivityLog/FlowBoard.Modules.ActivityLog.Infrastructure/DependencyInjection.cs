using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.ActivityLog.Infrastructure;

/// <summary>
/// Registers all ActivityLog module services: repositories, EF Core configurations,
/// and any module-specific infrastructure. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds ActivityLog module infrastructure services.</summary>
    public static IServiceCollection AddActivityLogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Phase N: register repositories, EF Core config assembly, module-specific services
        // AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
