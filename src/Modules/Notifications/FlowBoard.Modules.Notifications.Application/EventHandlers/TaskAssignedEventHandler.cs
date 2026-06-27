using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Notifications.Application.Commands.CreateNotification;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Events;
using MediatR;

namespace FlowBoard.Modules.Notifications.Application.EventHandlers;

/// <summary>
/// Reacts to <see cref="TaskAssignedEvent"/> (raised by the Tasks module and dispatched from the
/// outbox) by creating a <c>task_assigned</c> notification for the assignee. This is the
/// cross-module communication path: Tasks publishes a domain event, Notifications reacts.
/// </summary>
public sealed class TaskAssignedEventHandler(ISender sender)
    : INotificationHandler<DomainEventNotification<TaskAssignedEvent>>
{
    /// <inheritdoc/>
    public async Task Handle(
        DomainEventNotification<TaskAssignedEvent> notification,
        CancellationToken cancellationToken)
    {
        var @event = notification.DomainEvent;

        await sender.Send(
            new CreateNotificationCommand(
                @event.AssigneeId.Value,
                @event.OrganisationId.Value,
                NotificationType.TaskAssigned.DbValue,
                "You have been assigned a task.",
                "task",
                @event.TaskId.Value),
            cancellationToken);
    }
}
