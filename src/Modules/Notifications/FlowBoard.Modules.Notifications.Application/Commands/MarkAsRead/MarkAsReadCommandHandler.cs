using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Notifications.Application.Commands.MarkAsRead;

/// <summary>
/// Handles <see cref="MarkAsReadCommand"/>: marks the current user's notification read and invalidates
/// their cached unread count. A notification belonging to another user resolves to not-found.
/// </summary>
public sealed class MarkAsReadCommandHandler(
    ICurrentUserService currentUser,
    INotificationRepository notifications,
    ICacheService cache)
    : IRequestHandler<MarkAsReadCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await notifications.GetByIdAsync(
            NotificationId.From(request.NotificationId), currentUser.UserId, cancellationToken);

        if (notification is null)
            return Result.Failure(NotificationErrors.NotFound);

        notification.MarkAsRead();
        await cache.RemoveAsync(NotificationCacheKeys.Unread(currentUser.UserId.Value), cancellationToken);

        return Result.Success();
    }
}
