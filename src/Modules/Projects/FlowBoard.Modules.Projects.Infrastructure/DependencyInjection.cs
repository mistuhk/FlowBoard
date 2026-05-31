using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.Projects.Infrastructure;

/// <summary>
/// Registers all Projects module services: repositories, EF Core configurations,
/// and any module-specific infrastructure. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds Projects module infrastructure services.</summary>
    public static IServiceCollection AddProjectsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Phase N: register repositories, EF Core config assembly, module-specific services
        // AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
