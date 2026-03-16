namespace FlowBoard.Domain.Primitives;

/// <summary>
/// Marker interface for all domain events. Pure domain contract with no external dependencies.
/// The Application layer wraps these in MediatR notifications for dispatching.
/// Domain events are persisted to the outbox in the same DB transaction as the aggregate
/// change, then dispatched asynchronously by the <c>OutboxProcessor</c>.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier for this event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp at which the event was raised.</summary>
    DateTime OccurredAt { get; }
}
