using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlowBoard.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef migrations</c> when no running
/// application is available to resolve the <see cref="AppDbContext"/>.
/// Connection string targets the local Docker Postgres instance.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc/>
    public AppDbContext CreateDbContext(string[] args)
    {
        // At runtime each module registers its EF configuration assembly in AddXModule.
        // The design-time tooling never runs that DI wiring, so discover every module's
        // Infrastructure assembly from the startup project's output and register it here
        // otherwise module entities (e.g. users) are absent from the model and migrations.
        RegisterModuleConfigurationAssemblies();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=flowboard;Username=flowboard;Password=flowboard",
            npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }

    private static void RegisterModuleConfigurationAssemblies()
    {
        foreach (var path in Directory.GetFiles(
                     AppContext.BaseDirectory, "FlowBoard.Modules.*.Infrastructure.dll"))
        {
            AppDbContext.AddConfigurationAssembly(Assembly.LoadFrom(path));
        }
    }
}
