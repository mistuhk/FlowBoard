using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Projects.Domain.Repositories;
using FlowBoard.Modules.Projects.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.Projects.Infrastructure;

/// <summary>
/// Registers all Projects module services: the repository and the module's EF Core configuration
/// assembly. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds Projects module infrastructure services.</summary>
    public static IServiceCollection AddProjectsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Force the Application assembly to load so the central MediatR and FluentValidation
        // scans in Program.cs discover this module's handlers and validators.
        _ = Application.AssemblyReference.Assembly;

        services.AddScoped<IProjectRepository, ProjectRepository>();

        // Expose this assembly's IEntityTypeConfiguration implementations to the shared AppDbContext.
        AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
