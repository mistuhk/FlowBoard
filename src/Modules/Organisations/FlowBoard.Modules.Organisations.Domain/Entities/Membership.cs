using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Entities;

/// <summary>
/// A user's membership of an organisation. A child entity of the <c>Organisation</c>
/// aggregate: it is created, modified, and removed only through the aggregate root,
/// never constructed or mutated directly from outside the aggregate.
/// </summary>
public sealed class Membership : Entity<MembershipId>
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private Membership() { }

    private Membership(
        MembershipId id,
        OrganisationId organisationId,
        UserId userId,
        MemberRole role,
        UserId? invitedById,
        DateTime joinedAt) : base(id)
    {
        OrganisationId = organisationId;
        UserId = userId;
        Role = role;
        InvitedById = invitedById;
        JoinedAt = joinedAt;
    }

    /// <summary>The organisation this membership belongs to.</summary>
    public OrganisationId OrganisationId { get; private set; }

    /// <summary>The user who holds this membership.</summary>
    public UserId UserId { get; private set; }

    /// <summary>The member's role within the organisation.</summary>
    public MemberRole Role { get; private set; } = null!;

    /// <summary>The user who invited this member, or <c>null</c> for the founding owner.</summary>
    public UserId? InvitedById { get; private set; }

    /// <summary>UTC timestamp at which the user joined the organisation.</summary>
    public DateTime JoinedAt { get; private set; }

    /// <summary>
    /// Creates a new membership. Internal to the aggregate: callers go through
    /// <c>Organisation</c> behaviours rather than constructing memberships directly.
    /// </summary>
    /// <param name="organisationId">The organisation being joined.</param>
    /// <param name="userId">The user joining.</param>
    /// <param name="role">The role granted to the member.</param>
    /// <param name="invitedById">The inviting user, or <c>null</c> for the founding owner.</param>
    /// <returns>A new <see cref="Membership"/> instance.</returns>
    internal static Membership Create(
        OrganisationId organisationId,
        UserId userId,
        MemberRole role,
        UserId? invitedById) =>
        new(MembershipId.New(), organisationId, userId, role, invitedById, DateTime.UtcNow);

    /// <summary>
    /// Changes the member's role. Internal to the aggregate: role changes are authorised and
    /// applied through <c>Organisation.ChangeMemberRole</c> and <c>Organisation.TransferOwnership</c>.
    /// </summary>
    /// <param name="role">The new role.</param>
    internal void ChangeRole(MemberRole role) => Role = role;
}
