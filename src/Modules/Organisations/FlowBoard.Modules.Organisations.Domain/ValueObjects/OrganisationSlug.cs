using System.Text;
using System.Text.RegularExpressions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Organisations.Domain.ValueObjects;

/// <summary>
/// Value object representing an organisation's URL-safe slug.
/// Lowercase alphanumeric characters and single hyphens only, with no leading,
/// trailing, or consecutive hyphens. Uniqueness across organisations is enforced by
/// the database, not by this value object.
/// </summary>
public sealed partial class OrganisationSlug : ValueObject
{
    private const int MaxLength = 100;

    private OrganisationSlug(string value) => Value = value;

    /// <summary>The validated slug string.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a validated <see cref="OrganisationSlug"/> from an explicit slug string.
    /// </summary>
    /// <param name="value">The candidate slug.</param>
    /// <returns>A valid <see cref="OrganisationSlug"/> instance.</returns>
    /// <exception cref="DomainException">Thrown if the value is empty, too long, or not a valid slug.</exception>
    public static OrganisationSlug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Organisation slug cannot be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new DomainException($"Organisation slug must not exceed {MaxLength} characters.");

        if (!SlugPattern().IsMatch(trimmed))
            throw new DomainException($"'{value}' is not a valid slug. Use lowercase letters, digits, and single hyphens.");

        return new OrganisationSlug(trimmed);
    }

    /// <summary>
    /// Derives a slug from an organisation name by lowercasing, replacing each run of
    /// non-alphanumeric characters with a single hyphen, and trimming hyphens from the ends.
    /// </summary>
    /// <param name="name">The organisation name to derive the slug from.</param>
    /// <returns>A valid <see cref="OrganisationSlug"/> instance.</returns>
    /// <exception cref="DomainException">Thrown if the name contains no alphanumeric characters to derive a slug from.</exception>
    public static OrganisationSlug FromName(OrganisationName name)
    {
        var lowered = name.Value.ToLowerInvariant();
        var builder = new StringBuilder(lowered.Length);
        var previousWasHyphen = false;

        foreach (var character in lowered)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasHyphen = false;
            }
            else if (!previousWasHyphen)
            {
                builder.Append('-');
                previousWasHyphen = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        if (slug.Length > MaxLength)
            slug = slug[..MaxLength].Trim('-');

        if (slug.Length == 0)
            throw new DomainException($"Cannot derive a slug from organisation name '{name.Value}'.");

        return new OrganisationSlug(slug);
    }

    /// <summary>
    /// Rehydrates an <see cref="OrganisationSlug"/> from an already-validated value, used by
    /// EF Core when materialising a stored row. Skips validation, so it must only be called
    /// with values that were produced by <see cref="Create"/> or <see cref="FromName"/>.
    /// </summary>
    /// <param name="value">The stored, already-validated slug.</param>
    /// <returns>An <see cref="OrganisationSlug"/> wrapping the value.</returns>
    public static OrganisationSlug FromPersistence(string value) => new(value);

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
