using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Events;

/// <summary>
/// Raised when a new organisation is created via <c>Organisation.Create</c>.
/// The creator is registered as the sole owner. Consumed by other modules
/// (for example ActivityLog) to record the event.
/// </summary>
/// <param name="OrganisationId">The identifier of the newly created organisation.</param>
/// <param name="OwnerId">The identifier of the founding owner.</param>
public sealed record OrganisationCreatedEvent(OrganisationId OrganisationId, UserId OwnerId) : DomainEvent;
