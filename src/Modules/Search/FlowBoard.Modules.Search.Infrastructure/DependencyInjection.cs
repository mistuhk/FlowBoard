using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Search.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.Search.Infrastructure;

/// <summary>
/// Registers all Search module services: the full-text search repository. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds Search module infrastructure services.</summary>
    public static IServiceCollection AddSearchModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Force the Application assembly to load so the central MediatR scan in Program.cs discovers
        // this module's query handlers.
        _ = Application.AssemblyReference.Assembly;

        services.AddScoped<ISearchRepository, SearchRepository>();

        // Expose this assembly's IEntityTypeConfiguration implementations (the keyless read models
        // mapped to the tasks and projects tables via ToView) to the shared AppDbContext.
        AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
