using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Entities;

/// <summary>
/// A comment on a task. A child entity of the <see cref="Aggregates.TaskItem"/> aggregate: created,
/// edited, and removed only through the root. Soft-deleted (never hard-deleted).
/// </summary>
public sealed class Comment : Entity<CommentId>
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private Comment() { }

    private Comment(CommentId id, TaskId taskId, OrganisationId organisationId, UserId authorId, CommentContent content)
        : base(id)
    {
        TaskId = taskId;
        OrganisationId = organisationId;
        AuthorId = authorId;
        Content = content;
    }

    /// <summary>The task this comment belongs to.</summary>
    public TaskId TaskId { get; private set; }

    /// <summary>The organisation the comment belongs to (denormalised for tenant scoping).</summary>
    public OrganisationId OrganisationId { get; private set; }

    /// <summary>The author of the comment.</summary>
    public UserId AuthorId { get; private set; }

    /// <summary>The comment body.</summary>
    public CommentContent Content { get; private set; } = null!;

    /// <summary>
    /// UTC timestamp of the most recent edit. Database-managed by the <c>updated_at</c> trigger;
    /// never assigned in code.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>UTC timestamp at which the comment was soft-deleted. <c>null</c> if active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Whether the comment has been soft-deleted.</summary>
    public bool IsDeleted => DeletedAt is not null;

    /// <summary>Creates a new comment. Internal to the aggregate.</summary>
    internal static Comment Create(TaskId taskId, OrganisationId organisationId, UserId authorId, CommentContent content) =>
        new(CommentId.New(), taskId, organisationId, authorId, content);

    /// <summary>Replaces the comment body. Internal to the aggregate.</summary>
    internal void Edit(CommentContent content) => Content = content;

    /// <summary>Soft-deletes the comment. Internal to the aggregate.</summary>
    internal void Delete() => DeletedAt = DateTime.UtcNow;
}
