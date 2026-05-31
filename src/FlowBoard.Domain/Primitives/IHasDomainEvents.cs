namespace FlowBoard.Domain.Primitives;

/// <summary>
/// Non-generic marker interface implemented by <see cref="AggregateRoot{TId}"/>.
/// Allows the Unit of Work to discover all aggregate roots in the EF Core change
/// tracker without needing to know their specific ID type.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>Domain events raised during the current business operation.</summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all accumulated domain events. Called by the Unit of Work
    /// after events have been serialised to the outbox table.
    /// </summary>
    void ClearDomainEvents();
}
