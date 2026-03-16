namespace FlowBoard.Domain.Primitives;

/// <summary>
/// Base class for all domain entities.
/// An entity has a stable identity represented by its <see cref="Id"/> — two entities
/// with the same <typeparamref name="TId"/> and the same runtime type are considered equal,
/// regardless of the values of their other properties.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier (e.g. a strongly-typed ID struct).</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Initialises a new entity with the given identity.
    /// </summary>
    /// <param name="id">The unique identifier for this entity.</param>
    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Parameterless constructor required for EF Core materialisation.
    /// Not called directly from application code.
    /// </summary>
    protected Entity() { }

    /// <summary>The unique identifier for this entity.</summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>UTC timestamp at which this entity was created.</summary>
    public DateTime CreatedAt { get; protected set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    /// <summary>Returns <c>true</c> if both entities have the same type and identity.</summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    /// <summary>Returns <c>true</c> if the entities differ in type or identity.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
