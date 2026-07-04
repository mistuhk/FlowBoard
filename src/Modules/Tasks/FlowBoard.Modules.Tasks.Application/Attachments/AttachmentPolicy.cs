namespace FlowBoard.Modules.Tasks.Application.Attachments;

/// <summary>Upload constraints for task attachments.</summary>
public static class AttachmentPolicy
{
    /// <summary>Maximum permitted file size: 25 MB.</summary>
    public const long MaxSizeBytes = 25L * 1024 * 1024;

    /// <summary>The allowed MIME types (case-insensitive).</summary>
    public static readonly IReadOnlySet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/webp",
        "application/pdf", "text/plain", "text/csv", "application/zip",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    };

    /// <summary>Whether the given MIME type is permitted.</summary>
    public static bool IsAllowedMime(string mimeType) => AllowedMimeTypes.Contains(mimeType);
}

/// <summary>Builds and validates tenant-namespaced object-storage keys for attachments.</summary>
public static class AttachmentStorageKey
{
    /// <summary>Builds a key of the form <c>{orgId}/{taskId}/{uuid}/{fileName}</c>.</summary>
    public static string Build(Guid organisationId, Guid taskId, string fileName) =>
        $"{organisationId}/{taskId}/{Guid.NewGuid()}/{Path.GetFileName(fileName)}";

    /// <summary>Whether the key is namespaced under the given organisation and task.</summary>
    public static bool BelongsTo(string storageKey, Guid organisationId, Guid taskId) =>
        storageKey.StartsWith($"{organisationId}/{taskId}/", StringComparison.Ordinal);
}
