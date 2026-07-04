using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.RequestAttachmentUpload;

/// <summary>
/// Requests a pre-signed URL to upload a file directly to storage. No metadata is persisted yet; the
/// client uploads to the URL then calls confirm.
/// </summary>
/// <param name="TaskId">The task to attach the file to.</param>
/// <param name="FileName">The original file name.</param>
/// <param name="FileSizeBytes">The file size in bytes (must be within the size limit).</param>
/// <param name="MimeType">The MIME type (must be permitted).</param>
public sealed record RequestAttachmentUploadCommand(Guid TaskId, string FileName, long FileSizeBytes, string MimeType)
    : ICommand<Result<UploadUrlResponse>>;
