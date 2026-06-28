using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Notifications.Application.Queries.GetNotifications;

/// <summary>
/// Handles <see cref="GetNotificationsQuery"/>: returns a page of the current user's notifications
/// (newest first), fetching one extra row to detect a further page. Read-only.
/// </summary>
public sealed class GetNotificationsQueryHandler(
    ICurrentUserService currentUser,
    INotificationRepository notifications)
    : IRequestHandler<GetNotificationsQuery, NotificationPageResponse>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    /// <inheritdoc/>
    public async Task<NotificationPageResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.Limit ?? DefaultPageSize, 1, MaxPageSize);

        var found = await notifications.GetPageAsync(
            currentUser.UserId,
            NotificationCursor.Decode(request.Cursor),
            pageSize + 1,  // one extra row tells us whether a further page exists
            cancellationToken);

        var hasMore = found.Count > pageSize;
        var page = hasMore ? found.Take(pageSize).ToList() : found.ToList();
        var nextCursor = hasMore ? NotificationCursor.Encode(page[^1].CreatedAt) : null;

        return new NotificationPageResponse(page.Select(NotificationResponse.From).ToList(), nextCursor);
    }
}
