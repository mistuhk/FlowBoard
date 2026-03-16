using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Holds the resolved organisation (tenant) context for the current HTTP request.
/// Populated by <c>TenantResolutionMiddleware</c> from the authenticated user's JWT claims.
/// All repository queries use this to scope data access to the current organisation.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The identifier of the organisation resolved from the current request's JWT token.
    /// </summary>
    OrganisationId CurrentOrganisationId { get; }

    /// <summary>
    /// <c>true</c> if the tenant context has been successfully resolved for this request;
    /// <c>false</c> for unauthenticated or pre-authentication requests.
    /// </summary>
    bool IsResolved { get; }
}
