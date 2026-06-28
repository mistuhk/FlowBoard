using FlowBoard.Modules.Tasks.Domain.Entities;

namespace FlowBoard.Modules.Tasks.Application;

/// <summary>The public representation of a task attachment.</summary>
/// <param name="Id">The attachment id.</param>
/// <param name="TaskId">The task it belongs to.</param>
/// <param name="UploadedById">The uploader.</param>
/// <param name="FileName">The original file name.</param>
/// <param name="FileSizeBytes">The file size in bytes.</param>
/// <param name="MimeType">The MIME type.</param>
/// <param name="CreatedAt">When the attachment was created.</param>
public sealed record AttachmentResponse(
    Guid Id,
    Guid TaskId,
    Guid UploadedById,
    string FileName,
    long FileSizeBytes,
    string MimeType,
    DateTime CreatedAt)
{
    /// <summary>Maps an <see cref="Attachment"/> to its response representation.</summary>
    public static AttachmentResponse From(Attachment attachment) =>
        new(
            attachment.Id.Value,
            attachment.TaskId.Value,
            attachment.UploadedById.Value,
            attachment.FileName,
            attachment.FileSizeBytes,
            attachment.MimeType,
            attachment.CreatedAt);
}

/// <summary>A pre-signed upload URL and the storage key the client must confirm afterwards.</summary>
/// <param name="StorageKey">The object-storage key to upload to and then confirm.</param>
/// <param name="UploadUrl">The pre-signed PUT URL.</param>
/// <param name="ExpiresAtUtc">When the URL expires.</param>
public sealed record UploadUrlResponse(string StorageKey, string UploadUrl, DateTime ExpiresAtUtc);

/// <summary>A pre-signed download URL.</summary>
/// <param name="Url">The pre-signed GET URL.</param>
/// <param name="ExpiresAtUtc">When the URL expires.</param>
public sealed record DownloadUrlResponse(string Url, DateTime ExpiresAtUtc);
