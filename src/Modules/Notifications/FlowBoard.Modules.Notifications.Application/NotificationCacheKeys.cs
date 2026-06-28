namespace FlowBoard.Modules.Notifications.Application;

/// <summary>Cache key conventions for the Notifications module.</summary>
public static class NotificationCacheKeys
{
    /// <summary>
    /// Key for a user's unread-notification count: <c>{userId}:notifications:unread</c>. Invalidated
    /// whenever a notification is created or marked read for that user.
    /// </summary>
    public static string Unread(Guid userId) => $"{userId}:notifications:unread";
}
