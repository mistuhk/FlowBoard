using FlowBoard.Application.Abstractions;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Infrastructure.Jobs;
using FlowBoard.Modules.Organisations.Infrastructure.Persistence;
using FlowBoard.Modules.Organisations.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.Organisations.Infrastructure;

/// <summary>
/// Registers all Organisations module services: repositories and the module's EF Core
/// configuration assembly. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds Organisations module infrastructure services.</summary>
    public static IServiceCollection AddOrganisationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Force the Application assembly to load so the central MediatR and FluentValidation
        // scans in Program.cs discover this module's handlers and validators.
        _ = Application.AssemblyReference.Assembly;

        services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        services.AddScoped<IOrganisationMembershipReader, OrganisationMembershipReader>();

        // Hangfire resolves recurring jobs from the container; the hourly schedule is registered
        // in the API composition root.
        services.AddScoped<ExpireInvitationsJob>();

        // Expose this assembly's IEntityTypeConfiguration implementations to the shared AppDbContext.
        AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
