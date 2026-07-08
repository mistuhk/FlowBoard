namespace FlowBoard.Domain.Primitives;

/// <summary>
/// Base class for aggregate roots. Extends <see cref="Entity{TId}"/> with the ability
/// to collect domain events raised during a business operation.
/// <para>
/// Events are accumulated in memory during the handler, then serialised to the
/// <c>outbox_messages</c> table in the same DB transaction by the Unit of Work.
/// The <c>OutboxProcessor</c> background job dispatches them asynchronously.
/// All event handlers must be idempotent.
/// </para>
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Initialises the aggregate root with the given identity.</summary>
    /// <param name="id">The unique identifier for this aggregate.</param>
    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Parameterless constructor required for EF Core materialisation.
    /// Not called directly from application code.
    /// </summary>
    protected AggregateRoot() { }

    /// <summary>
    /// The domain events raised during the current business operation.
    /// Cleared by the Unit of Work after the events have been written to the outbox.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event, appending it to the in-memory collection.
    /// Called from aggregate behaviour methods rather than directly.
    /// </summary>
    /// <param name="domainEvent">The event to raise.</param>
    protected void Raise(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all accumulated domain events. Called by the Unit of Work
    /// after events have been written to the outbox.
    /// </summary>
    public void ClearDomainEvents() =>
        _domainEvents.Clear();
}
