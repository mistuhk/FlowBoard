using FlowBoard.Api.Middleware;
using FlowBoard.Api.Services;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.Behaviours;
using FlowBoard.Infrastructure;
using FlowBoard.Modules.Identity.Infrastructure;
using FlowBoard.Modules.Organisations.Infrastructure;
using FlowBoard.Modules.Projects.Infrastructure;
using FlowBoard.Modules.Tasks.Infrastructure;
using FlowBoard.Modules.Notifications.Infrastructure;
using FlowBoard.Modules.ActivityLog.Infrastructure;
using FlowBoard.Modules.Search.Infrastructure;
using Hangfire;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) => config
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName());

builder.Services.AddInfrastructure(builder.Configuration);

// Module infrastructure (repositories + EF configs per bounded context)
// Stub registrations, each module adds real services as it is built:
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddOrganisationsModule(builder.Configuration);
builder.Services.AddProjectsModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddActivityLogModule(builder.Configuration);
builder.Services.AddSearchModule(builder.Configuration);

builder.Services.AddScoped<ICurrentUserService, NullCurrentUserService>();
builder.Services.AddScoped<ITenantContext, NullTenantContext>();

// MediatR: Each module's AddXModule() call loads its assembly into the AppDomain.
// GetAssemblies() is called AFTER module registration to capture all of them.
builder.Services.AddMediatR(cfg =>
{
    var assemblies = AppDomain.CurrentDomain
        .GetAssemblies()
        .Where(a => a.FullName?.StartsWith("FlowBoard") == true)
        .ToArray();

    cfg.RegisterServicesFromAssemblies(assemblies);

    // Pipeline order matters, registered top to bottom:
    // Logging -> Validation -> Performance -> Transaction
    cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehaviour<,>));
});

// FluentValidation: discover all validators across all assemblies
builder.Services.AddValidatorsFromAssemblies(
    AppDomain.CurrentDomain
        .GetAssemblies()
        .Where(a => a.FullName?.StartsWith("FlowBoard") == true));

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "FlowBoard API",
        Version = "v1",
        Description = "Multi-tenant SaaS project management platform"
    });
});

// Health checks
builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Postgres")!,
        name: "postgres",
        tags: ["ready"])
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        tags: ["ready"]);

// HttpContextAccessor (required by CurrentUserService in Sprint 1)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware pipeline (order is significant)
app.UseMiddleware<ExceptionHandlingMiddleware>();   // Must be first

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowBoard API v1");
        c.RoutePrefix = "swagger";
    });
    app.UseHangfireDashboard("/hangfire");
}

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    };
});

app.UseHttpsRedirection();
app.UseAuthentication();                           // Sprint 1 - JWT middleware
app.UseMiddleware<TenantResolutionMiddleware>();   // Sprint 2 - tenant resolution
app.UseAuthorization();
app.MapControllers();

// Health endpoints
// /health/live  -> always returns 200 if the process is running (liveness probe)
// /health/ready -> returns 200 only when DB + Redis are reachable (readiness probe)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

/// <summary>
/// Exposes <c>Program</c> as a partial class so Testcontainers integration tests
/// can use <c>WebApplicationFactory&lt;Program&gt;</c>.
/// </summary>
public partial class Program { }
