using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Entities;

/// <summary>
/// A file attached to a task. A child entity of the <see cref="Aggregates.TaskItem"/> aggregate. Only
/// metadata is stored here; the bytes live in object storage at <see cref="StorageKey"/>. Soft-deleted,
/// then hard-deleted (from the database and storage) by a retention job.
/// </summary>
public sealed class Attachment
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private Attachment() { }

    private Attachment(
        AttachmentId id, TaskId taskId, OrganisationId organisationId, UserId uploadedById,
        string fileName, long fileSizeBytes, string mimeType, string storageKey)
    {
        Id = id;
        TaskId = taskId;
        OrganisationId = organisationId;
        UploadedById = uploadedById;
        FileName = fileName;
        FileSizeBytes = fileSizeBytes;
        MimeType = mimeType;
        StorageKey = storageKey;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>The attachment id.</summary>
    public AttachmentId Id { get; private set; }

    /// <summary>The task the attachment belongs to.</summary>
    public TaskId TaskId { get; private set; }

    /// <summary>The organisation the attachment belongs to (denormalised for tenant scoping).</summary>
    public OrganisationId OrganisationId { get; private set; }

    /// <summary>The user who uploaded the file.</summary>
    public UserId UploadedById { get; private set; }

    /// <summary>The original file name.</summary>
    public string FileName { get; private set; } = null!;

    /// <summary>The file size in bytes.</summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>The MIME type.</summary>
    public string MimeType { get; private set; } = null!;

    /// <summary>The object-storage key (<c>{orgId}/{taskId}/{uuid}/{filename}</c>).</summary>
    public string StorageKey { get; private set; } = null!;

    /// <summary>When the attachment was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>UTC timestamp at which the attachment was soft-deleted. <c>null</c> if active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Whether the attachment has been soft-deleted.</summary>
    public bool IsDeleted => DeletedAt is not null;

    /// <summary>Creates a new attachment. Internal to the aggregate.</summary>
    internal static Attachment Create(
        TaskId taskId, OrganisationId organisationId, UserId uploadedById,
        string fileName, long fileSizeBytes, string mimeType, string storageKey) =>
        new(AttachmentId.New(), taskId, organisationId, uploadedById, fileName, fileSizeBytes, mimeType, storageKey);

    /// <summary>Soft-deletes the attachment. Internal to the aggregate.</summary>
    internal void Delete() => DeletedAt = DateTime.UtcNow;
}
