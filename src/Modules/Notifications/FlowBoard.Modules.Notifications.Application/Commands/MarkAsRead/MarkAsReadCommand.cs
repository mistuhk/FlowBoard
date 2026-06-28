using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Notifications.Application.Commands.MarkAsRead;

/// <summary>Marks a single notification belonging to the current user as read.</summary>
/// <param name="NotificationId">The notification to mark read.</param>
public sealed record MarkAsReadCommand(Guid NotificationId) : ICommand<Result>;
