using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Domain.Aggregates;

namespace FlowBoard.Modules.Notifications.Domain.Repositories;

/// <summary>Persistence abstraction for the <see cref="Notification"/> aggregate.</summary>
public interface INotificationRepository
{
    /// <summary>Adds a newly created notification to the unit of work.</summary>
    /// <param name="notification">The notification to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked notification belonging to the given user, or <c>null</c> if it does not exist
    /// or belongs to another user. Scoping by recipient is the isolation boundary for notifications.
    /// </summary>
    /// <param name="id">The notification id.</param>
    /// <param name="userId">The recipient the notification must belong to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Notification?> GetByIdAsync(NotificationId id, UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of the user's notifications, newest first, optionally starting after the given
    /// created-at cursor. One extra row beyond the page size may be requested to detect a further page.
    /// </summary>
    /// <param name="userId">The recipient whose notifications to list.</param>
    /// <param name="createdBefore">Exclusive upper bound on created-at (the decoded cursor), or <c>null</c> for the first page.</param>
    /// <param name="limit">The maximum number of rows to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<Notification>> GetPageAsync(
        UserId userId, DateTime? createdBefore, int limit, CancellationToken cancellationToken = default);

    /// <summary>Counts the user's unread notifications.</summary>
    /// <param name="userId">The recipient.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<int> CountUnreadAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>Marks all of the user's unread notifications as read in a single set-based update.</summary>
    /// <param name="userId">The recipient.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of notifications updated.</returns>
    Task<int> MarkAllAsReadAsync(UserId userId, CancellationToken cancellationToken = default);
}
