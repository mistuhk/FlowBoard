namespace FlowBoard.Infrastructure.Outbox;

/// <summary>
/// Represents a domain event that has been serialised to the <c>outbox_messages</c> table
/// as part of the same database transaction that persisted the aggregate change.
/// The <c>OutboxProcessor</c> Hangfire job reads unprocessed records, deserialises the
/// payload, and publishes the event via MediatR. Records must never be deleted; set
/// <see cref="ProcessedAt"/> when complete and <see cref="Error"/> on failure.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>Unique identifier for this outbox record.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The name of the aggregate type that raised the event (e.g. <c>"Task"</c>).</summary>
    public string AggregateType { get; init; } = string.Empty;

    /// <summary>The identifier of the aggregate instance that raised the event.</summary>
    public Guid AggregateId { get; init; }

    /// <summary>
    /// The fully-qualified type name of the domain event, used to deserialise
    /// <see cref="Payload"/> back to the correct <see cref="FlowBoard.Domain.Primitives.IDomainEvent"/> subtype.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>JSON-serialised domain event payload.</summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>UTC timestamp at which this record was written.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp at which the <c>OutboxProcessor</c> successfully dispatched this event.
    /// <c>null</c> if not yet processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Records the error message if processing failed after all retry attempts.
    /// <c>null</c> if the record has not been attempted or completed successfully.
    /// </summary>
    public string? Error { get; set; }
}
