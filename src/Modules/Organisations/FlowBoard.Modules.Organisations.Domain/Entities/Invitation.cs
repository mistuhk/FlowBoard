using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Entities;

/// <summary>
/// An invitation for a person (identified by email) to join an organisation in a given role.
/// A child entity of the <c>Organisation</c> aggregate: it is created, accepted, and removed
/// only through the aggregate root. The raw invitation token is never stored; only its hash is
/// held, and the hash is computed outside the domain (in the Application layer).
/// </summary>
public sealed class Invitation : Entity<InvitationId>
{
    /// <summary>The lifetime of an invitation before it expires.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(48);

    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private Invitation() { }

    private Invitation(
        InvitationId id,
        OrganisationId organisationId,
        string invitedEmail,
        UserId invitedById,
        string tokenHash,
        MemberRole role) : base(id)
    {
        OrganisationId = organisationId;
        InvitedEmail = invitedEmail;
        InvitedById = invitedById;
        TokenHash = tokenHash;
        Role = role;
        ExpiresAt = CreatedAt.Add(Lifetime);
    }

    /// <summary>The organisation this invitation grants access to.</summary>
    public OrganisationId OrganisationId { get; private set; }

    /// <summary>The normalised (lowercase, trimmed) email address the invitation was sent to.</summary>
    public string InvitedEmail { get; private set; } = null!;

    /// <summary>The user who issued the invitation.</summary>
    public UserId InvitedById { get; private set; }

    /// <summary>The hash of the single-use invitation token. The raw token is never persisted.</summary>
    public string TokenHash { get; private set; } = null!;

    /// <summary>The role the invitee will hold once they accept.</summary>
    public MemberRole Role { get; private set; } = null!;

    /// <summary>UTC timestamp after which the invitation can no longer be accepted.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC timestamp at which the invitation was accepted. <c>null</c> while pending.</summary>
    public DateTime? AcceptedAt { get; private set; }

    /// <summary>
    /// Issues a new pending invitation. Internal to the aggregate: callers go through
    /// <c>Organisation.InviteMember</c> rather than constructing invitations directly.
    /// </summary>
    /// <param name="organisationId">The organisation being joined.</param>
    /// <param name="invitedEmail">The normalised email address of the invitee.</param>
    /// <param name="role">The role the invitee will hold once accepted.</param>
    /// <param name="invitedById">The user issuing the invitation.</param>
    /// <param name="tokenHash">The hash of the single-use token.</param>
    /// <returns>A new pending <see cref="Invitation"/>.</returns>
    internal static Invitation Issue(
        OrganisationId organisationId,
        string invitedEmail,
        MemberRole role,
        UserId invitedById,
        string tokenHash) =>
        new(InvitationId.New(), organisationId, invitedEmail, invitedById, tokenHash, role);

    /// <summary>Whether the invitation is still pending (not accepted and not expired) at the given instant.</summary>
    /// <param name="asOf">The instant to evaluate against, in UTC.</param>
    public bool IsPending(DateTime asOf) => AcceptedAt is null && asOf <= ExpiresAt;

    /// <summary>Marks the invitation as accepted at the given instant.</summary>
    /// <param name="acceptedAt">The acceptance timestamp, in UTC.</param>
    internal void MarkAccepted(DateTime acceptedAt) => AcceptedAt = acceptedAt;
}
