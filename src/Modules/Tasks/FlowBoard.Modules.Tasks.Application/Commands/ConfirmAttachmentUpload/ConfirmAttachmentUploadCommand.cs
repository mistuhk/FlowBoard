using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.ConfirmAttachmentUpload;

/// <summary>
/// Confirms a completed upload: verifies the object exists in storage and persists its metadata as an
/// attachment on the task.
/// </summary>
/// <param name="TaskId">The task the file was uploaded for.</param>
/// <param name="StorageKey">The storage key returned by the upload request.</param>
/// <param name="FileName">The original file name.</param>
/// <param name="FileSizeBytes">The file size in bytes.</param>
/// <param name="MimeType">The MIME type.</param>
public sealed record ConfirmAttachmentUploadCommand(
    Guid TaskId, string StorageKey, string FileName, long FileSizeBytes, string MimeType)
    : ICommand<Result<AttachmentResponse>>;
