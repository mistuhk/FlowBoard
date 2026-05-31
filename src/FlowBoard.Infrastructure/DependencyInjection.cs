using FlowBoard.Application.Abstractions;
using FlowBoard.Infrastructure.Caching;
using FlowBoard.Infrastructure.Persistence;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FlowBoard.Infrastructure;

/// <summary>
/// Registers all shared infrastructure services: EF Core, Redis, Hangfire.
/// Each module's own infrastructure is registered separately via the module's
/// <c>DependencyInjection</c> extension (e.g. <c>AddIdentityModule</c>).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds shared infrastructure services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' is not configured. " +
                "Add it to appsettings.Development.json or environment variables.");

        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException(
                "Connection string 'Redis' is not configured.");

        // EF Core
        services.AddDbContext<AppDbContext>((_, options) =>
            options
                .UseNpgsql(postgresConnectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                })
                .UseSnakeCaseNamingConvention()  // Maps C# PascalCase -> snake_case columns
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddScoped<ICacheService, RedisCacheService>();

        // Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(postgresConnectionString)));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 5;
            options.ServerName = $"flowboard-{Environment.MachineName}";
        });

        return services;
    }
}
