using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Events;

/// <summary>
/// Raised when a user joins an organisation by accepting an invitation via
/// <c>Organisation.AcceptInvitation</c>. Consumed by other modules (for example ActivityLog).
/// The role is carried by name so the event payload serialises cleanly to the outbox.
/// </summary>
/// <param name="OrganisationId">The organisation joined.</param>
/// <param name="UserId">The user who joined.</param>
/// <param name="Role">The name of the role the new member holds.</param>
public sealed record MemberJoinedEvent(
    OrganisationId OrganisationId,
    UserId UserId,
    string Role) : DomainEvent;
