using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application.Authorization;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.DeleteComment;

/// <summary>
/// Handles <see cref="DeleteCommentCommand"/>: loads the task, checks the caller is the comment's
/// author or an Admin/Owner, and soft-deletes the comment.
/// </summary>
public sealed class DeleteCommentCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks,
    IOrganisationMembershipReader membershipReader)
    : IRequestHandler<DeleteCommentCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var commentId = CommentId.From(request.CommentId);
        var comment = task.Comments.FirstOrDefault(c => c.Id == commentId && !c.IsDeleted);
        if (comment is null)
            return Result.Failure(TaskErrors.CommentNotFound);

        if (comment.AuthorId != currentUser.UserId
            && !await CommentModeration.IsAdminOrOwnerAsync(
                tenantContext.CurrentOrganisationId, currentUser.UserId, membershipReader, cancellationToken))
        {
            throw new ForbiddenException("Only the author or an Admin or Owner may delete this comment.");
        }

        task.DeleteComment(commentId);

        return Result.Success();
    }
}
