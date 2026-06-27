using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Events;

/// <summary>
/// Raised once per @handle in a new comment. The handle is a raw string; resolving it to a user (and
/// silently ignoring unknown handles) happens in the consuming handler, since the domain cannot look
/// users up.
/// </summary>
/// <param name="CommentId">The comment containing the mention.</param>
/// <param name="TaskId">The task the comment belongs to.</param>
/// <param name="OrganisationId">The organisation the task belongs to.</param>
/// <param name="AuthorId">The author of the comment (a self-mention is ignored downstream).</param>
/// <param name="Handle">The mentioned @handle, without the leading @, lower-cased.</param>
public sealed record UserMentionedEvent(
    CommentId CommentId,
    TaskId TaskId,
    OrganisationId OrganisationId,
    UserId AuthorId,
    string Handle) : DomainEvent;
