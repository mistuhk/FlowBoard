using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.AddComment;

/// <summary>Adds a comment to a task, authored by the current user.</summary>
/// <param name="TaskId">The task to comment on.</param>
/// <param name="Content">The comment body (1 to 10,000 characters).</param>
public sealed record AddCommentCommand(Guid TaskId, string Content) : ICommand<Result<CommentResponse>>;
