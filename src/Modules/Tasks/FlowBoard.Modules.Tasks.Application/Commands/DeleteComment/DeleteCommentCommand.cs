using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.DeleteComment;

/// <summary>Soft-deletes a comment. Permitted for the author, or an Admin or the Owner.</summary>
/// <param name="TaskId">The task the comment belongs to.</param>
/// <param name="CommentId">The comment to delete.</param>
public sealed record DeleteCommentCommand(Guid TaskId, Guid CommentId) : ICommand<Result>;
