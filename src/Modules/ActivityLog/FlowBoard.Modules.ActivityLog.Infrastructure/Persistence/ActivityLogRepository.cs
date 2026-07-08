using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.ActivityLog.Domain;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.ActivityLog.Infrastructure.Persistence;

/// <summary>EF Core implementation of <see cref="IActivityLogRepository"/>. Append-only: add and read.</summary>
internal sealed class ActivityLogRepository(AppDbContext context) : IActivityLogRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(ActivityLogEntry entry, CancellationToken cancellationToken = default) =>
        await context.Set<ActivityLogEntry>().AddAsync(entry, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ActivityLogEntry>> GetForEntityAsync(
        OrganisationId organisationId, string entityType, Guid entityId, DateTime? createdBefore, int limit, CancellationToken cancellationToken = default)
    {
        var query = context.Set<ActivityLogEntry>()
            .AsNoTracking()
            .Where(e => e.OrganisationId == organisationId && e.EntityType == entityType && e.EntityId == entityId);

        if (createdBefore.HasValue)
            query = query.Where(e => e.CreatedAt < createdBefore.Value);

        return await query.OrderByDescending(e => e.CreatedAt).Take(limit).ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ActivityLogEntry>> GetForActorAsync(
        OrganisationId organisationId, UserId actorId, DateTime? createdBefore, int limit, CancellationToken cancellationToken = default)
    {
        var query = context.Set<ActivityLogEntry>()
            .AsNoTracking()
            .Where(e => e.OrganisationId == organisationId && e.ActorId == actorId);

        if (createdBefore.HasValue)
            query = query.Where(e => e.CreatedAt < createdBefore.Value);

        return await query.OrderByDescending(e => e.CreatedAt).Take(limit).ToListAsync(cancellationToken);
    }
}
