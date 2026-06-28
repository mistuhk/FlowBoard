using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Notifications.Application.Commands.MarkAllAsRead;

/// <summary>Marks all of the current user's unread notifications as read.</summary>
public sealed record MarkAllAsReadCommand : ICommand<Result>;
