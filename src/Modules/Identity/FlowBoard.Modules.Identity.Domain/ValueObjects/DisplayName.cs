using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a user's display name.
/// Must be between 1 and 100 characters after trimming whitespace.
/// </summary>
public sealed class DisplayName : ValueObject
{
    private DisplayName(string value) => Value = value;

    /// <summary>The trimmed display name string.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a validated and trimmed <see cref="DisplayName"/> value object.
    /// </summary>
    /// <param name="value">The raw display name string provided by the user.</param>
    /// <returns>A valid <see cref="DisplayName"/> instance.</returns>
    /// <exception cref="DomainException">
    /// Thrown if the value is empty or exceeds 100 characters after trimming.
    /// </exception>
    public static DisplayName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Display name cannot be empty.");

        var trimmed = value.Trim();

        return trimmed.Length > 100
            ? throw new DomainException("Display name must not exceed 100 characters.")
            : new DisplayName(trimmed);
    }

    /// <summary>
    /// Rehydrates a <see cref="DisplayName"/> from an already-validated value, used by
    /// EF Core when materialising a stored row. Skips validation, so it must only be
    /// called with values that were produced by <see cref="Create"/>.
    /// </summary>
    /// <param name="trimmedValue">The stored, already-trimmed display name value.</param>
    /// <returns>A <see cref="DisplayName"/> wrapping the value.</returns>
    public static DisplayName FromPersistence(string trimmedValue) => new(trimmedValue);

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
