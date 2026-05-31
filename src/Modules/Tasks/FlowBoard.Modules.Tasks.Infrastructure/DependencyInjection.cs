using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.Tasks.Infrastructure;

/// <summary>
/// Registers all Tasks module services: repositories, EF Core configurations,
/// and any module-specific infrastructure. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds Tasks module infrastructure services.</summary>
    public static IServiceCollection AddTasksModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Phase N, register repositories, EF Core config assembly, module-specific services
        // AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
