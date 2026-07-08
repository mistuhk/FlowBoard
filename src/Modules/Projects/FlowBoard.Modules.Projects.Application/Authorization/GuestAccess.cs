using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Aggregates;

namespace FlowBoard.Modules.Projects.Application.Authorization;

/// <summary>
/// Encapsulates the Guest visibility rule: a Guest of an organisation can only see projects they
/// have been explicitly added to, whereas Members, Admins, and the Owner see all of the
/// organisation's projects. Used by the project read handlers.
/// </summary>
public static class GuestAccess
{
    // The organisation role name (as stored) whose visibility is restricted to explicit project membership.
    private const string GuestRoleName = "guest";

    /// <summary>Returns <c>true</c> if the caller holds the Guest role in the organisation.</summary>
    public static async Task<bool> IsGuestAsync(
        OrganisationId organisationId,
        UserId userId,
        IOrganisationMembershipReader membershipReader,
        CancellationToken cancellationToken)
    {
        var roleName = await membershipReader.GetRoleNameAsync(organisationId, userId, cancellationToken);
        return string.Equals(roleName, GuestRoleName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns <c>true</c> if the project must be hidden from the caller, that is, the caller is a
    /// Guest who has not been added to the project. Such a caller should receive a not-found result
    /// rather than a forbidden one, to avoid revealing that the project exists.
    /// </summary>
    public static async Task<bool> IsHiddenFromAsync(
        Project project,
        UserId userId,
        OrganisationId organisationId,
        IOrganisationMembershipReader membershipReader,
        CancellationToken cancellationToken) =>
        await IsGuestAsync(organisationId, userId, membershipReader, cancellationToken)
        && !project.HasMember(userId);
}
