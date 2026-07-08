using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Events;

/// <summary>
/// Raised when a member's role is changed via <c>Organisation.ChangeMemberRole</c>.
/// Roles are carried by name so the event payload serialises cleanly to the outbox.
/// </summary>
/// <param name="OrganisationId">The organisation the member belongs to.</param>
/// <param name="UserId">The member whose role changed.</param>
/// <param name="OldRole">The name of the member's previous role.</param>
/// <param name="NewRole">The name of the member's new role.</param>
public sealed record MemberRoleChangedEvent(
    OrganisationId OrganisationId,
    UserId UserId,
    string OldRole,
    string NewRole) : DomainEvent;
