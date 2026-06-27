using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using FlowBoard.Modules.Notifications.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.Notifications.Infrastructure;

/// <summary>
/// Registers all Notifications module services: the repository and the module's EF Core
/// configuration assembly. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds Notifications module infrastructure services.</summary>
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Force the Application assembly to load so the central MediatR and FluentValidation
        // scans in Program.cs discover this module's handlers and validators.
        _ = Application.AssemblyReference.Assembly;

        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Expose this assembly's IEntityTypeConfiguration implementations to the shared AppDbContext.
        AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
