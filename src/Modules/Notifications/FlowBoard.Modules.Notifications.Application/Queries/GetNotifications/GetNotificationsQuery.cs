using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.Notifications.Application.Queries.GetNotifications;

/// <summary>Lists the current user's notifications, newest first, cursor-paginated.</summary>
/// <param name="Cursor">An opaque cursor from a previous page, or <c>null</c> for the first page.</param>
/// <param name="Limit">The maximum page size (clamped server-side).</param>
public sealed record GetNotificationsQuery(string? Cursor, int? Limit) : IQuery<NotificationPageResponse>;
