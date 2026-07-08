using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Notifications.Application;

/// <summary>Expected, non-exceptional failures returned by the Notifications module.</summary>
public static class NotificationErrors
{
    /// <summary>Returned when a notification does not exist or does not belong to the current user.</summary>
    public static readonly Error NotFound = new(
        "Notifications.NotFound",
        "The notification could not be found.");
}
