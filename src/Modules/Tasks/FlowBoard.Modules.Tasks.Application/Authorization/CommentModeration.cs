using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Application.Authorization;

/// <summary>
/// Helper for comment edit/delete authorisation: a comment may be modified by its author, or by an
/// Admin or the Owner of the organisation. Role names are compared as strings to avoid referencing
/// the Organisations domain.
/// </summary>
public static class CommentModeration
{
    /// <summary>Returns <c>true</c> if the user is an Admin or the Owner of the organisation.</summary>
    public static async Task<bool> IsAdminOrOwnerAsync(
        OrganisationId organisationId,
        UserId userId,
        IOrganisationMembershipReader membershipReader,
        CancellationToken cancellationToken)
    {
        var roleName = await membershipReader.GetRoleNameAsync(organisationId, userId, cancellationToken);
        return string.Equals(roleName, "owner", StringComparison.OrdinalIgnoreCase)
            || string.Equals(roleName, "admin", StringComparison.OrdinalIgnoreCase);
    }
}
