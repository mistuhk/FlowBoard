using FlowBoard.Modules.Tasks.Domain.Entities;

namespace FlowBoard.Modules.Tasks.Application;

/// <summary>The public representation of a task comment.</summary>
/// <param name="Id">The comment id.</param>
/// <param name="TaskId">The task the comment belongs to.</param>
/// <param name="AuthorId">The author.</param>
/// <param name="Content">The comment body.</param>
/// <param name="Mentions">The @handles mentioned in the body.</param>
/// <param name="CreatedAt">When the comment was created.</param>
/// <param name="UpdatedAt">When the comment was last edited.</param>
public sealed record CommentResponse(
    Guid Id,
    Guid TaskId,
    Guid AuthorId,
    string Content,
    IReadOnlyList<string> Mentions,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>Maps a <see cref="Comment"/> entity to its response representation.</summary>
    public static CommentResponse From(Comment comment) =>
        new(
            comment.Id.Value,
            comment.TaskId.Value,
            comment.AuthorId.Value,
            comment.Content.Value,
            comment.Content.Mentions,
            comment.CreatedAt,
            comment.UpdatedAt);
}
