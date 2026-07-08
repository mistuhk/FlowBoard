using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Entities;
using FlowBoard.Modules.Organisations.Domain.Events;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Aggregates;

/// <summary>
/// Aggregate root for the Organisations bounded context.
/// Owns its <see cref="Membership"/> children and enforces every membership invariant,
/// the central one being that an organisation always has exactly one owner.
/// All membership state is reached through this root, never mutated directly.
/// </summary>
public sealed class Organisation : AggregateRoot<OrganisationId>
{
    private readonly List<Membership> _memberships = [];
    private readonly List<Invitation> _invitations = [];

    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private Organisation() { }

    /// <summary>The organisation's display name.</summary>
    public OrganisationName Name { get; private set; } = null!;

    /// <summary>The organisation's unique, URL-safe slug.</summary>
    public OrganisationSlug Slug { get; private set; } = null!;

    /// <summary>The user who currently owns the organisation.</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>The organisation's memberships. Read-only: mutated only through aggregate behaviour.</summary>
    public IReadOnlyList<Membership> Memberships => _memberships.AsReadOnly();

    /// <summary>The organisation's invitations. Read-only: mutated only through aggregate behaviour.</summary>
    public IReadOnlyList<Invitation> Invitations => _invitations.AsReadOnly();

    /// <summary>
    /// UTC timestamp of the most recent change to this aggregate.
    /// Database-managed: set on insert by the <c>updated_at</c> column default and on
    /// update by the organisation's <c>updated_at</c> trigger. Never assigned in code.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>UTC timestamp at which this organisation was soft-deleted. <c>null</c> if active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Creates a new organisation, registering the creator as its sole owner, and raises
    /// <see cref="OrganisationCreatedEvent"/>.
    /// </summary>
    /// <param name="name">The organisation's display name.</param>
    /// <param name="slug">The organisation's unique, URL-safe slug.</param>
    /// <param name="ownerId">The user creating the organisation, who becomes its owner.</param>
    /// <returns>A new <see cref="Organisation"/> with a single owner membership.</returns>
    public static Organisation Create(OrganisationName name, OrganisationSlug slug, UserId ownerId)
    {
        var organisation = new Organisation
        {
            Id = OrganisationId.New(),
            Name = name,
            Slug = slug,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,  // object initialiser bypasses the base ctor, so set it explicitly
            // updated_at is database-managed (column default on insert, trigger on update).
        };

        organisation._memberships.Add(
            Membership.Create(organisation.Id, ownerId, MemberRole.Owner, invitedById: null));

        organisation.Raise(new OrganisationCreatedEvent(organisation.Id, ownerId));
        return organisation;
    }

    /// <summary>
    /// Renames the organisation. Only the owner may rename it. The new name is already
    /// validated by <see cref="OrganisationName"/>.
    /// </summary>
    /// <param name="name">The new organisation name.</param>
    /// <param name="renamedById">The user attempting the rename.</param>
    /// <exception cref="ForbiddenException">Thrown if the caller is not the owner.</exception>
    public void Rename(OrganisationName name, UserId renamedById)
    {
        if (renamedById != OwnerId)
            throw new ForbiddenException("Only the organisation owner may rename the organisation.");

        Name = name;
    }

    /// <summary>
    /// Soft-deletes the organisation and raises <see cref="OrganisationDeletedEvent"/>.
    /// Only the owner may delete the organisation. Idempotent: deleting an already-deleted
    /// organisation is a no-op and raises no event.
    /// </summary>
    /// <param name="deletedById">The user attempting the deletion.</param>
    /// <exception cref="ForbiddenException">Thrown if the caller is not the owner.</exception>
    public void Delete(UserId deletedById)
    {
        if (DeletedAt is not null)
            return;

        if (deletedById != OwnerId)
            throw new ForbiddenException("Only the organisation owner may delete the organisation.");

        DeletedAt = DateTime.UtcNow;
        Raise(new OrganisationDeletedEvent(Id));
    }

    /// <summary>
    /// Invites a person (by email) to join the organisation in the given role, and raises
    /// <see cref="MemberInvitedEvent"/>. Only an Admin or the Owner may invite. The invitee
    /// cannot be invited as Owner, and a person with a still-pending invitation cannot be
    /// invited again.
    /// </summary>
    /// <param name="invitedEmail">The invitee's email address. Normalised (trimmed, lowercased).</param>
    /// <param name="role">The role the invitee will hold once they accept. Must not be Owner.</param>
    /// <param name="invitedById">The user issuing the invitation. Must be an Admin or the Owner.</param>
    /// <param name="tokenHash">The hash of the single-use invitation token, computed by the caller.</param>
    /// <exception cref="ForbiddenException">Thrown if the inviter is not an Admin or the Owner.</exception>
    /// <exception cref="DomainException">Thrown if the role is Owner or a pending invitation already exists for the email.</exception>
    public void InviteMember(string invitedEmail, MemberRole role, UserId invitedById, string tokenHash)
    {
        var inviter = MembershipFor(invitedById);
        if (inviter is null || !inviter.Role.IsAtLeast(MemberRole.Admin))
            throw new ForbiddenException("Only an Admin or the Owner may invite members.");

        if (role == MemberRole.Owner)
            throw new DomainException("A member cannot be invited as Owner.");

        var email = Normalise(invitedEmail);
        var now = DateTime.UtcNow;

        if (_invitations.Any(i => i.IsPending(now) && i.InvitedEmail == email))
            throw new DomainException("A pending invitation already exists for this email address.");

        _invitations.Add(Invitation.Issue(Id, email, role, invitedById, tokenHash));

        Raise(new MemberInvitedEvent(Id, email, invitedById, role.Name));
    }

    /// <summary>
    /// Accepts a pending invitation identified by its token hash, adding the accepting user as a
    /// member in the invitation's role, and raises <see cref="MemberJoinedEvent"/>.
    /// </summary>
    /// <param name="tokenHash">The hash of the token presented by the invitee.</param>
    /// <param name="userId">The authenticated user accepting the invitation.</param>
    /// <exception cref="DomainException">
    /// Thrown if no matching invitation exists, the invitation has expired or was already used,
    /// or the user is already a member.
    /// </exception>
    public void AcceptInvitation(string tokenHash, UserId userId)
    {
        var now = DateTime.UtcNow;

        var invitation = _invitations.FirstOrDefault(i => i.TokenHash == tokenHash)
            ?? throw new DomainException("The invitation is invalid.");

        if (!invitation.IsPending(now))
            throw new DomainException("The invitation has expired or has already been used.");

        if (MembershipFor(userId) is not null)
            throw new DomainException("The user is already a member of this organisation.");

        invitation.MarkAccepted(now);
        _memberships.Add(Membership.Create(Id, userId, invitation.Role, invitation.InvitedById));

        Raise(new MemberJoinedEvent(Id, userId, invitation.Role.Name));
    }

    /// <summary>
    /// Declines a pending invitation identified by its token hash, removing it. Raises no event.
    /// </summary>
    /// <param name="tokenHash">The hash of the token presented by the invitee.</param>
    /// <exception cref="DomainException">
    /// Thrown if no matching invitation exists or it has expired or was already used.
    /// </exception>
    public void DeclineInvitation(string tokenHash)
    {
        var invitation = _invitations.FirstOrDefault(i => i.TokenHash == tokenHash)
            ?? throw new DomainException("The invitation is invalid.");

        if (!invitation.IsPending(DateTime.UtcNow))
            throw new DomainException("The invitation has expired or has already been used.");

        _invitations.Remove(invitation);
    }

    /// <summary>
    /// Changes a member's role and raises <see cref="MemberRoleChangedEvent"/>. The caller must
    /// outrank both the member's current role and the new role, so no one can promote a member to
    /// their own level or above. Ownership is never assigned this way (use
    /// <see cref="TransferOwnership"/>). A no-op change (same role) raises no event.
    /// </summary>
    /// <param name="targetUserId">The member whose role is changing.</param>
    /// <param name="newRole">The new role. Must not be Owner.</param>
    /// <param name="changedById">The user making the change.</param>
    /// <exception cref="ForbiddenException">Thrown if the caller does not outrank the member and the new role.</exception>
    /// <exception cref="DomainException">Thrown if the new role is Owner or the target is not a member.</exception>
    public void ChangeMemberRole(UserId targetUserId, MemberRole newRole, UserId changedById)
    {
        if (newRole == MemberRole.Owner)
            throw new DomainException("Ownership is transferred, not assigned as a role.");

        var changer = MembershipFor(changedById)
            ?? throw new ForbiddenException("Only a member may change roles.");

        var target = MembershipFor(targetUserId)
            ?? throw new DomainException("The target user is not a member of this organisation.");

        if (!changer.Role.Outranks(target.Role) || !changer.Role.Outranks(newRole))
            throw new ForbiddenException("You must outrank both the member's current and new role.");

        if (target.Role == newRole)
            return;

        var oldRole = target.Role;
        target.ChangeRole(newRole);

        Raise(new MemberRoleChangedEvent(Id, targetUserId, oldRole.Name, newRole.Name));
    }

    /// <summary>
    /// Removes a member and raises <see cref="MemberRemovedEvent"/>. The Owner cannot be removed
    /// (transfer ownership first), and the caller must outrank the member being removed.
    /// </summary>
    /// <param name="targetUserId">The member to remove.</param>
    /// <param name="removedById">The user performing the removal.</param>
    /// <exception cref="ForbiddenException">Thrown if the caller does not outrank the member.</exception>
    /// <exception cref="DomainException">Thrown if the target is the Owner or not a member.</exception>
    public void RemoveMember(UserId targetUserId, UserId removedById)
    {
        var remover = MembershipFor(removedById)
            ?? throw new ForbiddenException("Only a member may remove members.");

        var target = MembershipFor(targetUserId)
            ?? throw new DomainException("The target user is not a member of this organisation.");

        if (target.Role == MemberRole.Owner)
            throw new DomainException("The owner cannot be removed. Transfer ownership first.");

        if (!remover.Role.Outranks(target.Role))
            throw new ForbiddenException("You must outrank the member you are removing.");

        _memberships.Remove(target);

        Raise(new MemberRemovedEvent(Id, targetUserId, removedById));
    }

    /// <summary>
    /// Transfers ownership to another existing member and raises
    /// <see cref="OwnershipTransferredEvent"/>. Only the current owner may transfer. The previous
    /// owner is demoted to Admin and the new owner takes the Owner role, preserving the invariant
    /// that there is exactly one owner.
    /// </summary>
    /// <param name="newOwnerId">The member to transfer ownership to.</param>
    /// <param name="currentOwnerId">The user attempting the transfer. Must be the current owner.</param>
    /// <exception cref="ForbiddenException">Thrown if the caller is not the current owner.</exception>
    /// <exception cref="DomainException">Thrown if the new owner is already the owner or is not a member.</exception>
    public void TransferOwnership(UserId newOwnerId, UserId currentOwnerId)
    {
        if (currentOwnerId != OwnerId)
            throw new ForbiddenException("Only the owner may transfer ownership.");

        if (newOwnerId == OwnerId)
            throw new DomainException("The nominated user is already the owner.");

        var newOwner = MembershipFor(newOwnerId)
            ?? throw new DomainException("Ownership can only be transferred to an existing member.");

        var currentOwner = MembershipFor(OwnerId)
            ?? throw new DomainException("The current owner membership is missing.");

        currentOwner.ChangeRole(MemberRole.Admin);
        newOwner.ChangeRole(MemberRole.Owner);
        OwnerId = newOwnerId;

        Raise(new OwnershipTransferredEvent(Id, currentOwnerId, newOwnerId));
    }

    private Membership? MembershipFor(UserId userId) =>
        _memberships.FirstOrDefault(m => m.UserId == userId);

    private static string Normalise(string email) => email.Trim().ToLowerInvariant();
}
