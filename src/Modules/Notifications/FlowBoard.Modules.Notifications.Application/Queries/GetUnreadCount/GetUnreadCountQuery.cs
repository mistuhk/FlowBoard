using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.Notifications.Application.Queries.GetUnreadCount;

/// <summary>Returns the current user's unread-notification count.</summary>
public sealed record GetUnreadCountQuery : IQuery<int>;
