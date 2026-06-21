using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Projects.Domain.ValueObjects;

/// <summary>
/// Value object representing a project's lifecycle status: Active or Archived. An archived project
/// is read-only (its details, tasks, and comments cannot be edited until it is restored).
/// </summary>
public sealed class ProjectStatus : ValueObject
{
    /// <summary>The project is active and editable.</summary>
    public static readonly ProjectStatus Active = new(nameof(Active));

    /// <summary>The project is archived and read-only.</summary>
    public static readonly ProjectStatus Archived = new(nameof(Archived));

    private ProjectStatus(string name) => Name = name;

    /// <summary>The canonical name of the status, used as its persisted value.</summary>
    public string Name { get; }

    /// <summary>All defined statuses.</summary>
    public static IReadOnlyList<ProjectStatus> All { get; } = [Active, Archived];

    /// <summary>
    /// Rehydrates a <see cref="ProjectStatus"/> from its persisted token, matching
    /// case-insensitively (statuses are stored in lowercase, for example <c>active</c>).
    /// </summary>
    /// <param name="value">The stored status token.</param>
    /// <exception cref="DomainException">Thrown if no status matches the token.</exception>
    public static ProjectStatus FromPersistence(string value) =>
        All.FirstOrDefault(status => string.Equals(status.Name, value, StringComparison.OrdinalIgnoreCase))
        ?? throw new DomainException($"'{value}' is not a recognised project status.");

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
