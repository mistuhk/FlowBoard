using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.AddComment;

/// <summary>
/// Handles <see cref="AddCommentCommand"/>: loads the task within the current organisation and adds
/// the comment authored by the caller.
/// </summary>
public sealed class AddCommentCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<AddCommentCommand, Result<CommentResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<CommentResponse>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<CommentResponse>(TaskErrors.NotFound);

        var comment = task.AddComment(currentUser.UserId, CommentContent.Create(request.Content));

        return Result.Success(CommentResponse.From(comment));
    }
}
