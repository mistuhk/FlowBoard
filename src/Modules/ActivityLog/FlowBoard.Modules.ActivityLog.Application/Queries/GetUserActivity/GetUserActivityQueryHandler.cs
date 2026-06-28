using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.ActivityLog.Domain;
using MediatR;

namespace FlowBoard.Modules.ActivityLog.Application.Queries.GetUserActivity;

/// <summary>
/// Handles <see cref="GetUserActivityQuery"/>: returns activity performed by the current user within
/// the current organisation, newest first. Read-only.
/// </summary>
public sealed class GetUserActivityQueryHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IActivityLogRepository activityLog)
    : IRequestHandler<GetUserActivityQuery, ActivityPageResponse>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    /// <inheritdoc/>
    public async Task<ActivityPageResponse> Handle(GetUserActivityQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.Limit ?? DefaultPageSize, 1, MaxPageSize);

        var found = await activityLog.GetForActorAsync(
            tenantContext.CurrentOrganisationId,
            currentUser.UserId,
            ActivityCursor.Decode(request.Cursor),
            pageSize + 1,
            cancellationToken);

        var hasMore = found.Count > pageSize;
        var page = hasMore ? found.Take(pageSize).ToList() : found.ToList();
        var nextCursor = hasMore ? ActivityCursor.Encode(page[^1].CreatedAt) : null;

        return new ActivityPageResponse(page.Select(ActivityLogResponse.From).ToList(), nextCursor);
    }
}
