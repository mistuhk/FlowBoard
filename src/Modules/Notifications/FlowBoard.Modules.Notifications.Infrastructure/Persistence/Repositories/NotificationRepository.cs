using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Notifications.Domain.Aggregates;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Notifications.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="INotificationRepository"/>.</summary>
internal sealed class NotificationRepository(AppDbContext context) : INotificationRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
        await context.Set<Notification>().AddAsync(notification, cancellationToken);

    /// <inheritdoc/>
    public async Task<Notification?> GetByIdAsync(
        NotificationId id, UserId userId, CancellationToken cancellationToken = default) =>
        await context.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Notification>> GetPageAsync(
        UserId userId, DateTime? createdBefore, int limit, CancellationToken cancellationToken = default)
    {
        var query = context.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (createdBefore.HasValue)
            query = query.Where(n => n.CreatedAt < createdBefore.Value);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CountUnreadAsync(UserId userId, CancellationToken cancellationToken = default) =>
        await context.Set<Notification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

    /// <inheritdoc/>
    public async Task<int> MarkAllAsReadAsync(UserId userId, CancellationToken cancellationToken = default) =>
        await context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), cancellationToken);
}
