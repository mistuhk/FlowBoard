using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application.Authorization;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.RemoveAttachment;

/// <summary>
/// Handles <see cref="RemoveAttachmentCommand"/>: loads the task, checks the caller is the uploader or
/// an Admin/Owner, and soft-deletes the attachment. The bytes are removed later by the retention job.
/// </summary>
public sealed class RemoveAttachmentCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks,
    IOrganisationMembershipReader membershipReader)
    : IRequestHandler<RemoveAttachmentCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RemoveAttachmentCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var attachmentId = AttachmentId.From(request.AttachmentId);
        var attachment = task.Attachments.FirstOrDefault(a => a.Id == attachmentId && !a.IsDeleted);
        if (attachment is null)
            return Result.Failure(TaskErrors.AttachmentNotFound);

        if (attachment.UploadedById != currentUser.UserId
            && !await CommentModeration.IsAdminOrOwnerAsync(
                tenantContext.CurrentOrganisationId, currentUser.UserId, membershipReader, cancellationToken))
        {
            throw new ForbiddenException("Only the uploader or an Admin or Owner may remove this attachment.");
        }

        task.RemoveAttachment(attachmentId);

        return Result.Success();
    }
}
