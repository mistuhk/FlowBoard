using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Events;

/// <summary>
/// Raised when an organisation is soft-deleted via <c>Organisation.Delete</c>.
/// Consumed by other modules to react to the organisation's removal.
/// </summary>
/// <param name="OrganisationId">The identifier of the soft-deleted organisation.</param>
public sealed record OrganisationDeletedEvent(OrganisationId OrganisationId) : DomainEvent;
