namespace FlowBoard.Modules.Tasks.Presentation.Contracts;

/// <summary>Request body for obtaining a pre-signed upload URL.</summary>
/// <param name="FileName">The original file name.</param>
/// <param name="FileSizeBytes">The file size in bytes.</param>
/// <param name="MimeType">The MIME type.</param>
public sealed record RequestAttachmentUploadRequest(string FileName, long FileSizeBytes, string MimeType);

/// <summary>Request body for confirming a completed upload.</summary>
/// <param name="StorageKey">The storage key returned by the upload request.</param>
/// <param name="FileName">The original file name.</param>
/// <param name="FileSizeBytes">The file size in bytes.</param>
/// <param name="MimeType">The MIME type.</param>
public sealed record ConfirmAttachmentUploadRequest(string StorageKey, string FileName, long FileSizeBytes, string MimeType);
