using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Notifications.Application.Commands.MarkAllAsRead;

/// <summary>
/// Handles <see cref="MarkAllAsReadCommand"/>: marks every unread notification for the current user as
/// read in one set-based update, then invalidates their cached unread count.
/// </summary>
public sealed class MarkAllAsReadCommandHandler(
    ICurrentUserService currentUser,
    INotificationRepository notifications,
    ICacheService cache)
    : IRequestHandler<MarkAllAsReadCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        await notifications.MarkAllAsReadAsync(currentUser.UserId, cancellationToken);
        await cache.RemoveAsync(NotificationCacheKeys.Unread(currentUser.UserId.Value), cancellationToken);

        return Result.Success();
    }
}
