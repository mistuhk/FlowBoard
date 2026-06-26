using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Notifications.Application.Commands.CreateNotification;

/// <summary>
/// Creates a notification for a user. Internal to the platform: dispatched by domain-event handlers
/// reacting to events from other modules, not exposed as a public endpoint.
/// </summary>
/// <param name="UserId">The recipient.</param>
/// <param name="OrganisationId">The organisation the notification belongs to.</param>
/// <param name="Type">The notification type token (for example <c>task_assigned</c>).</param>
/// <param name="Message">The human-readable message.</param>
/// <param name="EntityType">The referenced entity type (for example <c>task</c>), if any.</param>
/// <param name="EntityId">The referenced entity id, if any.</param>
public sealed record CreateNotificationCommand(
    Guid UserId,
    Guid OrganisationId,
    string Type,
    string Message,
    string? EntityType,
    Guid? EntityId) : ICommand<Result>;
