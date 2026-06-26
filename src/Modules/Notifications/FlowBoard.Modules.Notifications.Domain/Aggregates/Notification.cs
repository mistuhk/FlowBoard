using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;

namespace FlowBoard.Modules.Notifications.Domain.Aggregates;

/// <summary>
/// Aggregate root for the Notifications bounded context. A notification is created in reaction to a
/// domain event and is never edited or deleted, only marked as read.
/// </summary>
public sealed class Notification : AggregateRoot<NotificationId>
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private Notification() { }

    /// <summary>The user the notification is for.</summary>
    public UserId UserId { get; private set; }

    /// <summary>The organisation the notification belongs to.</summary>
    public OrganisationId OrganisationId { get; private set; }

    /// <summary>The kind of notification.</summary>
    public NotificationType Type { get; private set; } = null!;

    /// <summary>The human-readable message.</summary>
    public string Message { get; private set; } = null!;

    /// <summary>The type of the referenced entity (for example <c>task</c>), if any.</summary>
    public string? EntityType { get; private set; }

    /// <summary>The id of the referenced entity, if any.</summary>
    public Guid? EntityId { get; private set; }

    /// <summary>Whether the recipient has read the notification.</summary>
    public bool IsRead { get; private set; }

    /// <summary>Creates a new, unread notification.</summary>
    /// <param name="userId">The recipient.</param>
    /// <param name="organisationId">The organisation the notification belongs to.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="message">The human-readable message.</param>
    /// <param name="entityType">The referenced entity type, if any.</param>
    /// <param name="entityId">The referenced entity id, if any.</param>
    /// <returns>A new <see cref="Notification"/>.</returns>
    public static Notification Create(
        UserId userId,
        OrganisationId organisationId,
        NotificationType type,
        string message,
        string? entityType,
        Guid? entityId) =>
        new()
        {
            Id = NotificationId.New(),
            UserId = userId,
            OrganisationId = organisationId,
            Type = type,
            Message = message,
            EntityType = entityType,
            EntityId = entityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,  // object initialiser bypasses the base ctor
        };

    /// <summary>Marks the notification as read. Idempotent.</summary>
    public void MarkAsRead() => IsRead = true;
}
