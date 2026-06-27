using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Application.Abstractions;

/// <summary>The availability of a project for tenant-scoped operations such as creating a task.</summary>
public enum ProjectAvailability
{
    /// <summary>No active project with that id exists in the organisation.</summary>
    NotFound,

    /// <summary>The project exists but is archived, so it is read-only.</summary>
    Archived,

    /// <summary>The project exists and is active.</summary>
    Active,
}

/// <summary>
/// Read-only lookup of a project's availability within an organisation, used by other modules
/// (for example Tasks) that must validate a project without referencing the Projects domain.
/// Implemented in the Projects Infrastructure layer.
/// </summary>
public interface IProjectReader
{
    /// <summary>
    /// Returns whether the project exists in the organisation and, if so, whether it is active or
    /// archived. Soft-deleted projects are reported as <see cref="ProjectAvailability.NotFound"/>.
    /// </summary>
    /// <param name="projectId">The project to check.</param>
    /// <param name="organisationId">The organisation the caller is acting within.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<ProjectAvailability> GetAvailabilityAsync(
        ProjectId projectId,
        OrganisationId organisationId,
        CancellationToken cancellationToken = default);
}
