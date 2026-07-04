using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetAttachmentDownloadUrl;

/// <summary>Returns a short-lived pre-signed URL to download an attachment.</summary>
/// <param name="TaskId">The task the attachment belongs to.</param>
/// <param name="AttachmentId">The attachment to download.</param>
public sealed record GetAttachmentDownloadUrlQuery(Guid TaskId, Guid AttachmentId) : IQuery<Result<DownloadUrlResponse>>;
