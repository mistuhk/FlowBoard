using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.ActivityLog.Domain;

/// <summary>
/// An immutable, append-only record of something that happened in an organisation. Created in
/// reaction to a domain event and never modified or deleted. There is no aggregate root: an entry is
/// a standalone entity.
/// </summary>
public sealed class ActivityLogEntry
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private ActivityLogEntry() { }

    private ActivityLogEntry(
        Guid id,
        OrganisationId organisationId,
        string entityType,
        Guid entityId,
        string eventType,
        UserId? actorId,
        Dictionary<string, object>? metadata)
    {
        Id = id;
        OrganisationId = organisationId;
        EntityType = entityType;
        EntityId = entityId;
        EventType = eventType;
        ActorId = actorId;
        Metadata = metadata;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>The entry id.</summary>
    public Guid Id { get; private set; }

    /// <summary>The organisation the activity belongs to.</summary>
    public OrganisationId OrganisationId { get; private set; }

    /// <summary>The kind of entity acted upon (for example <c>Task</c>, <c>Project</c>, <c>Membership</c>).</summary>
    public string EntityType { get; private set; } = null!;

    /// <summary>The id of the entity acted upon.</summary>
    public Guid EntityId { get; private set; }

    /// <summary>The event token (for example <c>task.assigned</c>, <c>project.archived</c>).</summary>
    public string EventType { get; private set; } = null!;

    /// <summary>The user who performed the action, or <c>null</c> if system-generated.</summary>
    public UserId? ActorId { get; private set; }

    /// <summary>Additional context (old/new values, related ids), stored as JSONB.</summary>
    public Dictionary<string, object>? Metadata { get; private set; }

    /// <summary>UTC creation time. Set once on construction and never changed.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Creates a new activity-log entry.</summary>
    /// <param name="organisationId">The organisation the activity belongs to.</param>
    /// <param name="entityType">The kind of entity acted upon.</param>
    /// <param name="entityId">The id of the entity acted upon.</param>
    /// <param name="eventType">The event token.</param>
    /// <param name="actorId">The acting user, or <c>null</c> if system-generated.</param>
    /// <param name="metadata">Optional additional context.</param>
    /// <returns>A new <see cref="ActivityLogEntry"/>.</returns>
    public static ActivityLogEntry Create(
        OrganisationId organisationId,
        string entityType,
        Guid entityId,
        string eventType,
        UserId? actorId = null,
        Dictionary<string, object>? metadata = null) =>
        new(Guid.NewGuid(), organisationId, entityType, entityId, eventType, actorId, metadata);
}
