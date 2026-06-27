using FlowBoard.Modules.Notifications.Domain.Aggregates;

namespace FlowBoard.Modules.Notifications.Domain.Repositories;

/// <summary>Persistence abstraction for the <see cref="Notification"/> aggregate.</summary>
public interface INotificationRepository
{
    /// <summary>Adds a newly created notification to the unit of work.</summary>
    /// <param name="notification">The notification to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
}
