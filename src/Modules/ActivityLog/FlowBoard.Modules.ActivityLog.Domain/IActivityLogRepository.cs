using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.ActivityLog.Domain;

/// <summary>
/// Persistence abstraction for <see cref="ActivityLogEntry"/>. Append-only by design: entries can be
/// added and read, never updated or deleted.
/// </summary>
public interface IActivityLogRepository
{
    /// <summary>Adds a new entry to the unit of work.</summary>
    /// <param name="entry">The entry to append.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(ActivityLogEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of entries for a specific entity (newest first), used for the project feed.
    /// </summary>
    /// <param name="organisationId">The organisation to scope to.</param>
    /// <param name="entityType">The entity type to filter by.</param>
    /// <param name="entityId">The entity id to filter by.</param>
    /// <param name="createdBefore">Exclusive upper bound on created-at (the decoded cursor), or <c>null</c> for the first page.</param>
    /// <param name="limit">The maximum number of rows to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<ActivityLogEntry>> GetForEntityAsync(
        OrganisationId organisationId, string entityType, Guid entityId, DateTime? createdBefore, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of entries performed by a specific actor (newest first), used for the user feed.
    /// </summary>
    /// <param name="organisationId">The organisation to scope to.</param>
    /// <param name="actorId">The acting user to filter by.</param>
    /// <param name="createdBefore">Exclusive upper bound on created-at (the decoded cursor), or <c>null</c> for the first page.</param>
    /// <param name="limit">The maximum number of rows to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<ActivityLogEntry>> GetForActorAsync(
        OrganisationId organisationId, UserId actorId, DateTime? createdBefore, int limit, CancellationToken cancellationToken = default);
}
