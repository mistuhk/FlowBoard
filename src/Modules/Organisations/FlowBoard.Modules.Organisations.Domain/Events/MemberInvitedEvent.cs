using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Events;

/// <summary>
/// Raised when a member is invited to an organisation via <c>Organisation.InviteMember</c>.
/// Consumed by the Notifications module to send the invitation email.
/// The role is carried by name so the event payload serialises cleanly to the outbox.
/// </summary>
/// <param name="OrganisationId">The organisation the invitation is for.</param>
/// <param name="InvitedEmail">The normalised email address invited.</param>
/// <param name="InvitedById">The user who issued the invitation.</param>
/// <param name="Role">The name of the role the invitee will hold once accepted.</param>
public sealed record MemberInvitedEvent(
    OrganisationId OrganisationId,
    string InvitedEmail,
    UserId InvitedById,
    string Role) : DomainEvent;
