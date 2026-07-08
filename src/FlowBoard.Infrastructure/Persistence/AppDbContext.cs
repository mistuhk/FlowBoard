using FlowBoard.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FlowBoard.Infrastructure.Persistence;

/// <summary>
/// Single EF Core <see cref="DbContext"/> for the entire modular monolith.
/// <para>
/// Entity type configurations are discovered from this assembly (shared infrastructure)
/// plus any additional assemblies registered by module infrastructure layers via
/// <see cref="AddConfigurationAssembly"/>. Each module's <c>DependencyInjection</c>
/// calls this method when it registers its EF Core configurations.
/// </para>
/// </summary>
public sealed class AppDbContext : DbContext
{
    private static readonly List<Assembly> ConfigurationAssemblies = [];

    /// <summary>
    /// Registers an assembly containing <see cref="IEntityTypeConfiguration{TEntity}"/>
    /// implementations. Called by each module's infrastructure DI registration.
    /// </summary>
    /// <param name="assembly">The assembly to scan for EF Core configurations.</param>
    public static void AddConfigurationAssembly(Assembly assembly)
    {
        if (!ConfigurationAssemblies.Contains(assembly))
            ConfigurationAssemblies.Add(assembly);
    }

    /// <inheritdoc/>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Outbox messages for the transactional outbox pattern.</summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Always scan this (shared Infrastructure) assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Scan each module's Infrastructure assembly as they register themselves
        // Phase 1 adds: Identity.Infrastructure
        // Phase 2 adds: Organisations.Infrastructure
        // etc.
        foreach (var assembly in ConfigurationAssemblies)
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        base.OnModelCreating(modelBuilder);
    }
}
