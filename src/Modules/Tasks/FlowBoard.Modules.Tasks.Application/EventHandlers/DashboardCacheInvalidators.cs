using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Tasks.Application.Queries.GetDashboard;
using FlowBoard.Modules.Tasks.Domain.Events;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.EventHandlers;

/// <summary>
/// Invalidates the assignee's cached dashboard when a task is assigned to them, so their
/// assigned-task counts and workload refresh on the next read.
/// </summary>
public sealed class TaskAssignedDashboardInvalidator(ICacheService cache)
    : INotificationHandler<DomainEventNotification<TaskAssignedEvent>>
{
    /// <inheritdoc/>
    public async Task Handle(DomainEventNotification<TaskAssignedEvent> notification, CancellationToken cancellationToken) =>
        await cache.RemoveAsync(
            GetDashboardQueryHandler.CacheKey(notification.DomainEvent.AssigneeId.Value), cancellationToken);
}

/// <summary>Invalidates the previous assignee's cached dashboard when a task is unassigned from them.</summary>
public sealed class TaskUnassignedDashboardInvalidator(ICacheService cache)
    : INotificationHandler<DomainEventNotification<TaskUnassignedEvent>>
{
    /// <inheritdoc/>
    public async Task Handle(DomainEventNotification<TaskUnassignedEvent> notification, CancellationToken cancellationToken) =>
        await cache.RemoveAsync(
            GetDashboardQueryHandler.CacheKey(notification.DomainEvent.PreviousAssigneeId.Value), cancellationToken);
}
