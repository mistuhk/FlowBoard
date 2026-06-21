using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Events;

/// <summary>
/// Raised when a member is removed from an organisation via <c>Organisation.RemoveMember</c>.
/// </summary>
/// <param name="OrganisationId">The organisation the member was removed from.</param>
/// <param name="UserId">The member who was removed.</param>
/// <param name="RemovedById">The user who performed the removal.</param>
public sealed record MemberRemovedEvent(
    OrganisationId OrganisationId,
    UserId UserId,
    UserId RemovedById) : DomainEvent;
