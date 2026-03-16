using FlowBoard.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Register modules, MediatR, FluentValidation, EF Core, Redis,
//       Hangfire, Serilog, OpenTelemetry, Auth, Health Checks

// Pipeline
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();

// Expose Program for Testcontainers integration tests
public partial class Program { }
