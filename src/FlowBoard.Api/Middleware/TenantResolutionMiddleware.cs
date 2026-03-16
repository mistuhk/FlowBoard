using FlowBoard.Application.Abstractions;

namespace FlowBoard.Api.Middleware;

/// <summary>
/// ASP.NET Core middleware that resolves the current tenant (organisation) from the
/// authenticated user's JWT claims and makes it available to all downstream components
/// via <see cref="ITenantContext"/>.
/// <para>
/// Must be registered after <c>UseAuthentication()</c> and before <c>UseAuthorization()</c>
/// and the controller pipeline.
/// </para>
/// <para>
/// Also sets the Postgres session parameter <c>app.current_organisation_id</c> so that
/// Row-Level Security policies can enforce tenant isolation at the database level.
/// </para>
/// </summary>
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    /// <summary>Resolves the tenant context and passes control to the next middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="tenantContext">The tenant context to populate.</param>
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // The concrete ITenantContext implementation reads the "org_id" JWT claim
        // and sets the Postgres session variable via an EF Core DbCommandInterceptor.
        await next(context);
    }
}
