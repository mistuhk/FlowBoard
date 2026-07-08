using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Organisations.Domain.ValueObjects;

/// <summary>
/// Value object representing an organisation's display name.
/// Must be between 2 and 100 characters after trimming whitespace.
/// </summary>
public sealed class OrganisationName : ValueObject
{
    private OrganisationName(string value) => Value = value;

    /// <summary>The trimmed organisation name string.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a validated and trimmed <see cref="OrganisationName"/> value object.
    /// </summary>
    /// <param name="value">The raw organisation name provided by the caller.</param>
    /// <returns>A valid <see cref="OrganisationName"/> instance.</returns>
    /// <exception cref="DomainException">
    /// Thrown if the value is shorter than 2 characters or longer than 100 characters after trimming.
    /// </exception>
    public static OrganisationName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Organisation name cannot be empty.");

        var trimmed = value.Trim();

        return trimmed.Length switch
        {
            < 2 => throw new DomainException("Organisation name must be at least 2 characters."),
            > 100 => throw new DomainException("Organisation name must not exceed 100 characters."),
            _ => new OrganisationName(trimmed),
        };
    }

    /// <summary>
    /// Rehydrates an <see cref="OrganisationName"/> from an already-validated value, used by
    /// EF Core when materialising a stored row. Skips validation, so it must only be called
    /// with values that were produced by <see cref="Create"/>.
    /// </summary>
    /// <param name="trimmedValue">The stored, already-trimmed organisation name.</param>
    /// <returns>An <see cref="OrganisationName"/> wrapping the value.</returns>
    public static OrganisationName FromPersistence(string trimmedValue) => new(trimmedValue);

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
