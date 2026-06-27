using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.Tasks.Infrastructure;

/// <summary>
/// Registers all Tasks module services: the repository and the module's EF Core configuration
/// assembly. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds Tasks module infrastructure services.</summary>
    public static IServiceCollection AddTasksModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Force the Application assembly to load so the central MediatR and FluentValidation
        // scans in Program.cs discover this module's handlers and validators.
        _ = Application.AssemblyReference.Assembly;

        services.AddScoped<ITaskRepository, TaskRepository>();

        // Expose this assembly's IEntityTypeConfiguration implementations to the shared AppDbContext.
        AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
