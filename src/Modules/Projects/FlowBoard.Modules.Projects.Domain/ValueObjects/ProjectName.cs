using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Projects.Domain.ValueObjects;

/// <summary>
/// Value object representing a project's name. Must be between 1 and 150 characters after
/// trimming whitespace.
/// </summary>
public sealed class ProjectName : ValueObject
{
    private ProjectName(string value) => Value = value;

    /// <summary>The trimmed project name.</summary>
    public string Value { get; }

    /// <summary>Creates a validated and trimmed <see cref="ProjectName"/>.</summary>
    /// <param name="value">The raw project name.</param>
    /// <returns>A valid <see cref="ProjectName"/> instance.</returns>
    /// <exception cref="DomainException">Thrown if the value is empty or exceeds 150 characters.</exception>
    public static ProjectName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Project name cannot be empty.");

        var trimmed = value.Trim();

        return trimmed.Length > 150
            ? throw new DomainException("Project name must not exceed 150 characters.")
            : new ProjectName(trimmed);
    }

    /// <summary>
    /// Rehydrates a <see cref="ProjectName"/> from an already-validated value, used by EF Core when
    /// materialising a stored row. Skips validation.
    /// </summary>
    /// <param name="trimmedValue">The stored, already-trimmed project name.</param>
    public static ProjectName FromPersistence(string trimmedValue) => new(trimmedValue);

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
