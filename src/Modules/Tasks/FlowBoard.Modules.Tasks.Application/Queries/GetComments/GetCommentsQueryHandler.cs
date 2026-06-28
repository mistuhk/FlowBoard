using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetComments;

/// <summary>
/// Handles <see cref="GetCommentsQuery"/>: returns the task's active comments, oldest first.
/// Read-only. Soft-deleted comments are excluded in memory (owned types carry no query filter).
/// </summary>
public sealed class GetCommentsQueryHandler(
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<GetCommentsQuery, Result<IReadOnlyList<CommentResponse>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<CommentResponse>>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<IReadOnlyList<CommentResponse>>(TaskErrors.NotFound);

        IReadOnlyList<CommentResponse> comments = task.Comments
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .Select(CommentResponse.From)
            .ToList();

        return Result.Success(comments);
    }
}
