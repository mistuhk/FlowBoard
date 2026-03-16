using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Persistence;

/// <summary>
/// Single EF Core <see cref="DbContext"/> for the entire application.
/// Each module registers its own entity type configurations by implementing
/// <see cref="IEntityTypeConfiguration{TEntity}"/> in its Infrastructure project.
/// Configurations are discovered automatically via
/// <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discovers all IEntityTypeConfiguration<T> implementations
        // registered across all module Infrastructure assemblies.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
