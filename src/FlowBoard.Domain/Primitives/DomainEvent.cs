namespace FlowBoard.Domain.Primitives;

/// <summary>
/// Base record for all domain events. Provides default implementations of
/// <see cref="IDomainEvent.EventId"/> and <see cref="IDomainEvent.OccurredAt"/>.
/// Concrete events should be <c>sealed record</c> types that inherit this class
/// and carry their event-specific payload as positional parameters.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
