using FlowBoard.Modules.Notifications.Domain.Aggregates;

namespace FlowBoard.Modules.Notifications.Application;

/// <summary>The public representation of a notification.</summary>
/// <param name="Id">The notification id.</param>
/// <param name="Type">The notification type token (for example <c>task_assigned</c>).</param>
/// <param name="Message">The human-readable message.</param>
/// <param name="EntityType">The referenced entity type, if any.</param>
/// <param name="EntityId">The referenced entity id, if any.</param>
/// <param name="IsRead">Whether the notification has been read.</param>
/// <param name="OrganisationId">The organisation the notification belongs to.</param>
/// <param name="CreatedAt">When the notification was created.</param>
public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Message,
    string? EntityType,
    Guid? EntityId,
    bool IsRead,
    Guid OrganisationId,
    DateTime CreatedAt)
{
    /// <summary>Maps a <see cref="Notification"/> to its response representation.</summary>
    public static NotificationResponse From(Notification notification) =>
        new(
            notification.Id.Value,
            notification.Type.DbValue,
            notification.Message,
            notification.EntityType,
            notification.EntityId,
            notification.IsRead,
            notification.OrganisationId.Value,
            notification.CreatedAt);
}

/// <summary>A cursor-paginated page of notifications.</summary>
/// <param name="Items">The notifications on this page, newest first.</param>
/// <param name="NextCursor">An opaque cursor for the next page, or <c>null</c> if this is the last page.</param>
public sealed record NotificationPageResponse(IReadOnlyList<NotificationResponse> Items, string? NextCursor);
