using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Events;

/// <summary>
/// Raised when a comment is added to a task via <c>TaskItem.AddComment</c>. Consumed by ActivityLog,
/// and (for each handle) by the mention pipeline. Mentions are carried as plain handle strings.
/// </summary>
/// <param name="CommentId">The new comment.</param>
/// <param name="TaskId">The task commented on.</param>
/// <param name="OrganisationId">The organisation the task belongs to.</param>
/// <param name="AuthorId">The comment author.</param>
/// <param name="Mentions">The @handles mentioned in the comment (without the leading @).</param>
public sealed record CommentAddedEvent(
    CommentId CommentId,
    TaskId TaskId,
    OrganisationId OrganisationId,
    UserId AuthorId,
    IReadOnlyList<string> Mentions) : DomainEvent;
