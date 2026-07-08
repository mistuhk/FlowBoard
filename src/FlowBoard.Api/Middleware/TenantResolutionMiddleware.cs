using FlowBoard.Api.Services;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Api.Middleware;

/// <summary>
/// Resolves the current tenant (organisation) for org-scoped requests from the <c>orgId</c> route
/// value and populates <see cref="TenantContext"/>. Org-owned resources are nested under
/// <c>/api/v1/organisations/{orgId}/...</c>; requests without an <c>orgId</c> route value (for
/// example authentication, or the organisation collection itself) pass through unscoped.
/// <para>
/// This middleware only establishes which tenant a request targets. Whether the caller may act on
/// that tenant is enforced separately by the organisation authorisation policies, so a request for
/// an organisation the caller does not belong to is set here but rejected at authorisation before
/// any data is read.
/// </para>
/// <para>
/// Must run after <c>UseRouting</c> (so route values are populated) and before the endpoint executes.
/// </para>
/// </summary>
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    /// <summary>Resolves the tenant context and passes control to the next middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="tenantContext">The request-scoped tenant context to populate.</param>
    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (context.Request.RouteValues.TryGetValue("orgId", out var raw)
            && Guid.TryParse(raw?.ToString(), out var organisationId))
        {
            tenantContext.SetOrganisation(OrganisationId.From(organisationId));
        }

        await next(context);
    }
}
