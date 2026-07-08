using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetAttachmentDownloadUrl;

/// <summary>
/// Handles <see cref="GetAttachmentDownloadUrlQuery"/>: returns a pre-signed download URL for an active
/// attachment. Read-only.
/// </summary>
public sealed class GetAttachmentDownloadUrlQueryHandler(
    ITenantContext tenantContext,
    ITaskRepository tasks,
    IStorageService storage)
    : IRequestHandler<GetAttachmentDownloadUrlQuery, Result<DownloadUrlResponse>>
{
    private static readonly TimeSpan DownloadUrlExpiry = TimeSpan.FromMinutes(15);

    /// <inheritdoc/>
    public async Task<Result<DownloadUrlResponse>> Handle(GetAttachmentDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<DownloadUrlResponse>(TaskErrors.NotFound);

        var attachmentId = AttachmentId.From(request.AttachmentId);
        var attachment = task.Attachments.FirstOrDefault(a => a.Id == attachmentId && !a.IsDeleted);
        if (attachment is null)
            return Result.Failure<DownloadUrlResponse>(TaskErrors.AttachmentNotFound);

        var url = await storage.GenerateDownloadUrlAsync(attachment.StorageKey, DownloadUrlExpiry);

        return Result.Success(new DownloadUrlResponse(url, DateTime.UtcNow.Add(DownloadUrlExpiry)));
    }
}
