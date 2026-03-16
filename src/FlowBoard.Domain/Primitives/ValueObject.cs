namespace FlowBoard.Domain.Primitives;

/// <summary>
/// Base class for value objects. Value objects have no identity, two instances
/// with the same components are considered equal.
/// <para>
/// Subclasses must implement <see cref="GetEqualityComponents"/> to declare which
/// properties participate in equality comparison.
/// </para>
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the sequence of values that define equality for this value object.
    /// All significant properties should be yielded here.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc/>
    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is ValueObject other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) =>
                HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    /// <summary>Returns <c>true</c> if both value objects have equal components.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    /// <summary>Returns <c>true</c> if the value objects differ in at least one component.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
