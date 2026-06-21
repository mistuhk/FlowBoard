using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Read-only lookup of a user's role within an organisation, used by tenant-scoped authorisation.
/// Implemented in the Organisations Infrastructure layer; the role is returned by name (for example
/// <c>owner</c> or <c>admin</c>) so this abstraction stays free of the Organisations domain types.
/// </summary>
public interface IOrganisationMembershipReader
{
    /// <summary>
    /// Returns the caller's role name within the organisation, or <c>null</c> if the user is not an
    /// active member (or the organisation does not exist or is soft-deleted).
    /// </summary>
    /// <param name="organisationId">The organisation to check membership of.</param>
    /// <param name="userId">The user whose membership is being resolved.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<string?> GetRoleNameAsync(
        OrganisationId organisationId,
        UserId userId,
        CancellationToken cancellationToken = default);
}
