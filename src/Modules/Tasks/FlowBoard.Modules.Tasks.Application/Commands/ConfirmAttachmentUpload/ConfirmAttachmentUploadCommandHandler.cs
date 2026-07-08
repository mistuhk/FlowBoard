using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application.Attachments;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.ConfirmAttachmentUpload;

/// <summary>
/// Handles <see cref="ConfirmAttachmentUploadCommand"/>: checks the storage key is namespaced to this
/// task, verifies the object was actually uploaded, then records the attachment metadata.
/// </summary>
public sealed class ConfirmAttachmentUploadCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks,
    IStorageService storage)
    : IRequestHandler<ConfirmAttachmentUploadCommand, Result<AttachmentResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<AttachmentResponse>> Handle(ConfirmAttachmentUploadCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<AttachmentResponse>(TaskErrors.NotFound);

        if (!AttachmentStorageKey.BelongsTo(request.StorageKey, tenantContext.CurrentOrganisationId.Value, request.TaskId))
            return Result.Failure<AttachmentResponse>(TaskErrors.InvalidStorageKey);

        if (!await storage.ExistsAsync(request.StorageKey, cancellationToken))
            return Result.Failure<AttachmentResponse>(TaskErrors.AttachmentNotUploaded);

        var attachment = task.AddAttachment(
            currentUser.UserId, request.FileName, request.FileSizeBytes, request.MimeType, request.StorageKey);

        return Result.Success(AttachmentResponse.From(attachment));
    }
}
