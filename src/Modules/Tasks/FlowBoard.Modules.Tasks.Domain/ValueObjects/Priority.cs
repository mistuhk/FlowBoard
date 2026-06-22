using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Tasks.Domain.ValueObjects;

/// <summary>
/// Value object for a task's priority: Low, Medium, High, or Critical. Persisted as a lowercase token.
/// </summary>
public sealed class Priority : ValueObject
{
    /// <summary>Low priority.</summary>
    public static readonly Priority Low = new(nameof(Low));

    /// <summary>Medium priority (the default).</summary>
    public static readonly Priority Medium = new(nameof(Medium));

    /// <summary>High priority.</summary>
    public static readonly Priority High = new(nameof(High));

    /// <summary>Critical priority.</summary>
    public static readonly Priority Critical = new(nameof(Critical));

    private Priority(string name) => Name = name;

    /// <summary>The priority name (for example <c>High</c>), used for display and equality.</summary>
    public string Name { get; }

    /// <summary>All defined priorities.</summary>
    public static IReadOnlyList<Priority> All { get; } = [Low, Medium, High, Critical];

    /// <summary>Rehydrates a <see cref="Priority"/> from its persisted token, matching case-insensitively.</summary>
    /// <param name="value">The stored priority token.</param>
    /// <exception cref="DomainException">Thrown if no priority matches the token.</exception>
    public static Priority FromPersistence(string value) =>
        All.FirstOrDefault(priority => string.Equals(priority.Name, value, StringComparison.OrdinalIgnoreCase))
        ?? throw new DomainException($"'{value}' is not a recognised priority.");

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
