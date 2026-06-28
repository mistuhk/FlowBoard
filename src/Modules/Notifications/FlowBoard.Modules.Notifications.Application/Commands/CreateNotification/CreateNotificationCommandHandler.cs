using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Domain.Aggregates;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Notifications.Application.Commands.CreateNotification;

/// <summary>Handles <see cref="CreateNotificationCommand"/>: creates and stores a notification.</summary>
public sealed class CreateNotificationCommandHandler(
    INotificationRepository notifications,
    ICacheService cache)
    : IRequestHandler<CreateNotificationCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = Notification.Create(
            UserId.From(request.UserId),
            OrganisationId.From(request.OrganisationId),
            NotificationType.FromPersistence(request.Type),
            request.Message,
            request.EntityType,
            request.EntityId);

        await notifications.AddAsync(notification, cancellationToken);

        // The recipient's unread count has changed; drop the cached value so it is recomputed.
        await cache.RemoveAsync(NotificationCacheKeys.Unread(request.UserId), cancellationToken);

        return Result.Success();
    }
}
