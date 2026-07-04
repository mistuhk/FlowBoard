using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application.Attachments;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.RequestAttachmentUpload;

/// <summary>
/// Handles <see cref="RequestAttachmentUploadCommand"/>: confirms the task exists, mints a
/// tenant-namespaced storage key, and returns a short-lived pre-signed upload URL. No bytes touch the
/// API and nothing is persisted until the client confirms.
/// </summary>
public sealed class RequestAttachmentUploadCommandHandler(
    ITenantContext tenantContext,
    ITaskRepository tasks,
    IStorageService storage)
    : IRequestHandler<RequestAttachmentUploadCommand, Result<UploadUrlResponse>>
{
    private static readonly TimeSpan UploadUrlExpiry = TimeSpan.FromMinutes(5);

    /// <inheritdoc/>
    public async Task<Result<UploadUrlResponse>> Handle(RequestAttachmentUploadCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<UploadUrlResponse>(TaskErrors.NotFound);

        var storageKey = AttachmentStorageKey.Build(
            tenantContext.CurrentOrganisationId.Value, request.TaskId, request.FileName);

        var url = await storage.GenerateUploadUrlAsync(storageKey, request.MimeType, UploadUrlExpiry);

        return Result.Success(new UploadUrlResponse(storageKey, url, DateTime.UtcNow.Add(UploadUrlExpiry)));
    }
}
