using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a valid, normalised email address.
/// Construction validates format and normalises to lowercase.
/// Equality is case-insensitive.
/// </summary>
public sealed class Email : ValueObject
{
    private Email(string value) => Value = value;

    /// <summary>The normalised (lowercase, trimmed) email address string.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a validated and normalised <see cref="Email"/> value object.
    /// </summary>
    /// <param name="value">The raw email address string provided by the user.</param>
    /// <returns>A valid <see cref="Email"/> instance.</returns>
    /// <exception cref="DomainException">Thrown if the value is empty or not a valid email format.</exception>
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email address cannot be empty.");

        var normalised = value.Trim().ToLowerInvariant();

        if (!normalised.Contains('@') || normalised.Length > 254)
            throw new DomainException($"'{value}' is not a valid email address.");

        return new Email(normalised);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
