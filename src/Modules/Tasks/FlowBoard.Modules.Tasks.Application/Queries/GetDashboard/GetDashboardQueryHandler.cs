using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Tasks.Application.Abstractions;
using FlowBoard.Modules.Tasks.Application.Dashboard;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetDashboard;

/// <summary>
/// Handles <see cref="GetDashboardQuery"/>: returns the dashboard from the Redis cache when warm,
/// otherwise builds it from the read side and caches it. The cache is invalidated on task
/// assignment and unassignment; the short TTL is a backstop. Read-only.
/// </summary>
public sealed class GetDashboardQueryHandler(
    ICurrentUserService currentUser,
    IDashboardReader reader,
    ICacheService cache)
    : IRequestHandler<GetDashboardQuery, DashboardResponse>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    /// <summary>The cache key for a user's dashboard.</summary>
    public static string CacheKey(Guid userId) => $"{userId}:dashboard";

    /// <inheritdoc/>
    public async Task<DashboardResponse> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId.Value;
        var key = CacheKey(userId);

        var cached = await cache.GetAsync<DashboardResponse>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var dashboard = await reader.GetAsync(userId, cancellationToken);
        await cache.SetAsync(key, dashboard, CacheTtl, cancellationToken);

        return dashboard;
    }
}
