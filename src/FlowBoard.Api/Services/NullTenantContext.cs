using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Api.Services;

/// <summary>
/// No-op implementation of <see cref="ITenantContext"/> for Phase 0.
/// Replaced by the real implementation in Phase 2 (TenantResolutionMiddleware).
/// </summary>
internal sealed class NullTenantContext : ITenantContext
{
    /// <inheritdoc/>
    public OrganisationId CurrentOrganisationId => OrganisationId.From(Guid.Empty);

    /// <inheritdoc/>
    public bool IsResolved => false;
}
