using FlowBoard.Application.Abstractions;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Identity.Application.Abstractions;
using FlowBoard.Modules.Identity.Domain.Abstractions;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Infrastructure.Persistence.Repositories;
using FlowBoard.Modules.Identity.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Modules.Identity.Infrastructure;

/// <summary>
/// Registers all Identity module services: repositories, the password hasher, and the
/// module's EF Core configuration assembly. Called from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds Identity module infrastructure services.</summary>
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Force the Application assembly to load so the central MediatR and FluentValidation
        // scans in Program.cs discover this module's handlers and validators.
        _ = Application.AssemblyReference.Assembly;

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserDirectory, Persistence.UserDirectory>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

        // Access token issuance. The RSA key and JwtOptions are registered by the API's
        // AddJwtAuthentication; this generator resolves them at request time.
        services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();

        // Expose this assembly's IEntityTypeConfiguration implementations to the shared AppDbContext.
        AppDbContext.AddConfigurationAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
