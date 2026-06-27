using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Notifications.Application.Commands.CreateNotification;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Events;
using MediatR;

namespace FlowBoard.Modules.Notifications.Application.EventHandlers;

/// <summary>
/// Reacts to <see cref="UserMentionedEvent"/> (one per @handle in a new comment, dispatched from the
/// outbox) by creating a <c>user_mentioned</c> notification for the mentioned member. Unknown handles
/// resolve to no user and are silently ignored; a self-mention does not notify the author.
/// </summary>
public sealed class UserMentionedEventHandler(ISender sender, IUserDirectory userDirectory)
    : INotificationHandler<DomainEventNotification<UserMentionedEvent>>
{
    /// <inheritdoc/>
    public async Task Handle(
        DomainEventNotification<UserMentionedEvent> notification,
        CancellationToken cancellationToken)
    {
        var @event = notification.DomainEvent;

        var mentionedUserId = await userDirectory.ResolveHandleAsync(
            @event.OrganisationId, @event.Handle, cancellationToken);

        // Ignore handles that match no member, and do not notify someone for mentioning themselves.
        if (mentionedUserId is null || mentionedUserId == @event.AuthorId.Value)
            return;

        await sender.Send(
            new CreateNotificationCommand(
                mentionedUserId.Value,
                @event.OrganisationId.Value,
                NotificationType.UserMentioned.DbValue,
                "You were mentioned in a comment.",
                "task",
                @event.TaskId.Value),
            cancellationToken);
    }
}
