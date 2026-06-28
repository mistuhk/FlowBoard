using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Notifications.Application.Queries.GetUnreadCount;

/// <summary>
/// Handles <see cref="GetUnreadCountQuery"/>: returns the unread count from the Redis cache, falling
/// back to a database count (and re-populating the cache) on a miss. The cached value is invalidated
/// whenever a notification is created or marked read, with a short TTL as a safety net.
/// </summary>
public sealed class GetUnreadCountQueryHandler(
    ICurrentUserService currentUser,
    INotificationRepository notifications,
    ICacheService cache)
    : IRequestHandler<GetUnreadCountQuery, int>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    /// <inheritdoc/>
    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var key = NotificationCacheKeys.Unread(currentUser.UserId.Value);

        var cached = await cache.GetAsync<int?>(key, cancellationToken);
        if (cached.HasValue)
            return cached.Value;

        var count = await notifications.CountUnreadAsync(currentUser.UserId, cancellationToken);
        await cache.SetAsync(key, count, CacheTtl, cancellationToken);

        return count;
    }
}
