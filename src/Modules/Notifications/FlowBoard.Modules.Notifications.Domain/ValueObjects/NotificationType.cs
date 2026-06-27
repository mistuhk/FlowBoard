using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Notifications.Domain.ValueObjects;

/// <summary>
/// Value object for the kind of notification. Persisted as a snake_case token.
/// </summary>
public sealed class NotificationType : ValueObject
{
    /// <summary>A task was assigned to the user.</summary>
    public static readonly NotificationType TaskAssigned = new(nameof(TaskAssigned), "task_assigned");

    /// <summary>The user was mentioned in a comment.</summary>
    public static readonly NotificationType UserMentioned = new(nameof(UserMentioned), "user_mentioned");

    /// <summary>The user was invited to a project.</summary>
    public static readonly NotificationType ProjectInvited = new(nameof(ProjectInvited), "project_invited");

    /// <summary>A task's status changed.</summary>
    public static readonly NotificationType TaskStatusChanged = new(nameof(TaskStatusChanged), "task_status_changed");

    /// <summary>A comment was added.</summary>
    public static readonly NotificationType CommentAdded = new(nameof(CommentAdded), "comment_added");

    /// <summary>A task became blocked.</summary>
    public static readonly NotificationType TaskBlocked = new(nameof(TaskBlocked), "task_blocked");

    private NotificationType(string name, string dbValue)
    {
        Name = name;
        DbValue = dbValue;
    }

    /// <summary>The type name (for example <c>TaskAssigned</c>), used for display and equality.</summary>
    public string Name { get; }

    /// <summary>The persisted token (for example <c>task_assigned</c>).</summary>
    public string DbValue { get; }

    /// <summary>All defined notification types.</summary>
    public static IReadOnlyList<NotificationType> All { get; } =
        [TaskAssigned, UserMentioned, ProjectInvited, TaskStatusChanged, CommentAdded, TaskBlocked];

    /// <summary>Rehydrates a <see cref="NotificationType"/> from its persisted token.</summary>
    /// <param name="dbValue">The stored token.</param>
    /// <exception cref="DomainException">Thrown if no type matches the token.</exception>
    public static NotificationType FromPersistence(string dbValue) =>
        All.FirstOrDefault(type => type.DbValue == dbValue)
        ?? throw new DomainException($"'{dbValue}' is not a recognised notification type.");

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
