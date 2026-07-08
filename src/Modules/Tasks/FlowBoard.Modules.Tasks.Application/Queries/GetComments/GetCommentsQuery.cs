using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetComments;

/// <summary>Lists the active comments on a task, oldest first.</summary>
/// <param name="TaskId">The task whose comments to list.</param>
public sealed record GetCommentsQuery(Guid TaskId) : IQuery<Result<IReadOnlyList<CommentResponse>>>;
