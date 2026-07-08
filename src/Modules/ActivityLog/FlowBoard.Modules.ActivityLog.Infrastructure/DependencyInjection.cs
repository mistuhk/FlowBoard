using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.ActivityLog.Domain;
using FlowBoard.Modules.ActivityLog.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.ActivityLog.Infrastructure;

/// <summary>
/// Registers all ActivityLog module services: the repository and the module's EF Core configuration
/// assembly. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds ActivityLog module infrastructure services.</summary>
    public static IServiceCollection AddActivityLogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Force the Application assembly to load so the central MediatR and FluentValidation scans in
        // Program.cs discover this module's event handlers and validators.
        _ = Application.AssemblyReference.Assembly;

        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

        // Expose this assembly's IEntityTypeConfiguration implementations to the shared AppDbContext.
        AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
