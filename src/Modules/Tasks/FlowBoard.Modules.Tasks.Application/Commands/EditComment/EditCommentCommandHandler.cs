using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application.Authorization;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.EditComment;

/// <summary>
/// Handles <see cref="EditCommentCommand"/>: loads the task, checks the caller is the comment's
/// author or an Admin/Owner, and edits the body.
/// </summary>
public sealed class EditCommentCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks,
    IOrganisationMembershipReader membershipReader)
    : IRequestHandler<EditCommentCommand, Result<CommentResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<CommentResponse>> Handle(EditCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<CommentResponse>(TaskErrors.NotFound);

        var commentId = CommentId.From(request.CommentId);
        var comment = task.Comments.FirstOrDefault(c => c.Id == commentId && !c.IsDeleted);
        if (comment is null)
            return Result.Failure<CommentResponse>(TaskErrors.CommentNotFound);

        if (comment.AuthorId != currentUser.UserId
            && !await CommentModeration.IsAdminOrOwnerAsync(
                tenantContext.CurrentOrganisationId, currentUser.UserId, membershipReader, cancellationToken))
        {
            throw new ForbiddenException("Only the author or an Admin or Owner may edit this comment.");
        }

        task.EditComment(commentId, CommentContent.Create(request.Content));

        return Result.Success(CommentResponse.From(task.Comments.First(c => c.Id == commentId)));
    }
}
