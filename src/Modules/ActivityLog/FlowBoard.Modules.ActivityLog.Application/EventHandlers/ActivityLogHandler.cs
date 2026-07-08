using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.ActivityLog.Application.Commands.LogActivity;
using MediatR;

namespace FlowBoard.Modules.ActivityLog.Application.EventHandlers;

/// <summary>
/// Base for the per-event activity handlers. Each subclass maps one domain event to a
/// <see cref="LogActivityCommand"/>; this base dispatches it so the write rides the command pipeline.
/// One handler per event keeps the mapping accurate (the generic <see cref="DomainEvent"/> base
/// carries no organisation, entity, or actor information).
/// </summary>
/// <typeparam name="TEvent">The domain event reacted to.</typeparam>
public abstract class ActivityLogHandler<TEvent>(ISender sender)
    : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : DomainEvent
{
    /// <summary>Maps the event to the activity entry to append.</summary>
    /// <param name="event">The domain event.</param>
    protected abstract LogActivityCommand Map(TEvent @event);

    /// <inheritdoc/>
    public async Task Handle(DomainEventNotification<TEvent> notification, CancellationToken cancellationToken) =>
        await sender.Send(Map(notification.DomainEvent), cancellationToken);
}
