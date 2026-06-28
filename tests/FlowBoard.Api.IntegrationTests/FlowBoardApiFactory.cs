using FlowBoard.Application.Abstractions;
using FlowBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Spins up real PostgreSQL and Redis containers and boots the API against them, so the
/// full request pipeline (validation, transaction, EF Core, Redis) is exercised end to end.
/// The schema is created by applying the EF Core migrations on start.
/// </summary>
public sealed class FlowBoardApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("flowboard")
        .WithUsername("flowboard")
        .WithPassword("flowboard")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    /// <summary>The connection string of the running Redis container, for direct assertions.</summary>
    public string RedisConnectionString => _redis.GetConnectionString();

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", _redis.GetConnectionString());

        // No SMTP server in the test environment: swap the queueing email service for a no-op so
        // registrations and assignments do not enqueue Hangfire jobs that would fail to reach MailHog.
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, FakeEmailService>();
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}
