namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Abstraction for S3-compatible object storage operations.
/// File bytes never pass through the API server — clients upload directly to storage
/// using a pre-signed URL obtained from <see cref="GenerateUploadUrlAsync"/>.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Generates a pre-signed URL that allows a client to upload a file directly
    /// to object storage without routing bytes through the API server.
    /// </summary>
    /// <param name="storageKey">The destination key in object storage (e.g. <c>{orgId}/{taskId}/{uuid}/{filename}</c>).</param>
    /// <param name="contentType">The MIME type of the file being uploaded.</param>
    /// <param name="expiry">How long the pre-signed URL remains valid.</param>
    /// <returns>A pre-signed upload URL.</returns>
    Task<string> GenerateUploadUrlAsync(string storageKey, string contentType, TimeSpan expiry);

    /// <summary>
    /// Generates a pre-signed URL that allows a client to download a stored file.
    /// </summary>
    /// <param name="storageKey">The key of the object in storage.</param>
    /// <param name="expiry">How long the pre-signed URL remains valid.</param>
    /// <returns>A pre-signed download URL.</returns>
    Task<string> GenerateDownloadUrlAsync(string storageKey, TimeSpan expiry);

    /// <summary>
    /// Returns whether an object exists in storage. Used to confirm a client has actually uploaded a
    /// file before its metadata is persisted.
    /// </summary>
    /// <param name="storageKey">The key of the object to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes an object from storage.
    /// Called by the maintenance job when a soft-deleted attachment passes its retention window.
    /// </summary>
    /// <param name="storageKey">The key of the object to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
