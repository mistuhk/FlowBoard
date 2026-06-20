using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Repositories;

/// <summary>
/// Persistence abstraction for the <see cref="Organisation"/> aggregate.
/// Implemented in the Organisations Infrastructure layer; the Domain owns the contract.
/// </summary>
public interface IOrganisationRepository
{
    /// <summary>
    /// Determines whether an active organisation already uses the given slug.
    /// </summary>
    /// <param name="slug">The slug to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the slug is already taken; otherwise <c>false</c>.</returns>
    Task<bool> ExistsBySlugAsync(OrganisationSlug slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked <see cref="Organisation"/> by its identifier, with its memberships, or
    /// <c>null</c> if no active (non-deleted) organisation exists with that id. The entity is
    /// change-tracked so callers can mutate it and have the change persisted by the unit of work.
    /// </summary>
    /// <param name="id">The identifier of the organisation to load.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Organisation?> GetByIdAsync(OrganisationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a newly created <see cref="Organisation"/> to the unit of work. The change is not
    /// persisted until the surrounding transaction calls <c>SaveChangesAsync</c>.
    /// </summary>
    /// <param name="organisation">The organisation aggregate to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(Organisation organisation, CancellationToken cancellationToken = default);
}
