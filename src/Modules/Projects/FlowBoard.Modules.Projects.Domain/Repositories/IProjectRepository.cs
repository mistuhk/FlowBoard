using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Aggregates;

namespace FlowBoard.Modules.Projects.Domain.Repositories;

/// <summary>
/// Persistence abstraction for the <see cref="Project"/> aggregate. Every lookup is scoped to an
/// organisation, so a project is only ever returned to a caller acting within its owning tenant.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Loads a tracked <see cref="Project"/> by id, but only if it belongs to the given
    /// organisation and is not soft-deleted. Returns <c>null</c> otherwise (a project in another
    /// organisation is indistinguishable from one that does not exist).
    /// </summary>
    /// <param name="id">The project id.</param>
    /// <param name="organisationId">The organisation the caller is acting within.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Project?> GetByIdAsync(
        ProjectId id,
        OrganisationId organisationId,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the active (non-deleted) projects in an organisation.</summary>
    /// <param name="organisationId">The organisation to list projects for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<Project>> ListByOrganisationAsync(
        OrganisationId organisationId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a newly created <see cref="Project"/> to the unit of work.</summary>
    /// <param name="project">The project to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
}
