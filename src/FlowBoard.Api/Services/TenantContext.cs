using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Api.Services;

/// <summary>
/// Request-scoped implementation of <see cref="ITenantContext"/>. Populated by
/// <c>TenantResolutionMiddleware</c> from the route's organisation id, then read by repositories
/// and by <c>UnitOfWork</c> (to set the Postgres <c>app.current_organisation_id</c> session
/// variable for Row-Level Security).
/// </summary>
public sealed class TenantContext : ITenantContext
{
    /// <inheritdoc/>
    public OrganisationId CurrentOrganisationId { get; private set; }

    /// <inheritdoc/>
    public bool IsResolved { get; private set; }

    /// <summary>Sets the resolved organisation for the current request.</summary>
    /// <param name="organisationId">The organisation the request is scoped to.</param>
    public void SetOrganisation(OrganisationId organisationId)
    {
        CurrentOrganisationId = organisationId;
        IsResolved = true;
    }
}
