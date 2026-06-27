using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.EditComment;

/// <summary>Edits a comment's body. Permitted for the author, or an Admin or the Owner.</summary>
/// <param name="TaskId">The task the comment belongs to.</param>
/// <param name="CommentId">The comment to edit.</param>
/// <param name="Content">The new body (1 to 10,000 characters).</param>
public sealed record EditCommentCommand(Guid TaskId, Guid CommentId, string Content)
    : ICommand<Result<CommentResponse>>;
