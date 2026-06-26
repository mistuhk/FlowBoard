using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Notifications.Domain.Aggregates;
using FlowBoard.Modules.Notifications.Domain.Repositories;

namespace FlowBoard.Modules.Notifications.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="INotificationRepository"/>.</summary>
internal sealed class NotificationRepository(AppDbContext context) : INotificationRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
        await context.Set<Notification>().AddAsync(notification, cancellationToken);
}
