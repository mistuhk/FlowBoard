namespace FlowBoard.Infrastructure.Storage;

/// <summary>Object-storage settings, bound from the <c>Storage</c> configuration section.</summary>
public sealed class StorageOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Storage";

    /// <summary>The S3-compatible endpoint (MinIO locally, e.g. <c>http://localhost:9000</c>).</summary>
    public string Endpoint { get; init; } = "http://localhost:9000";

    /// <summary>The access key.</summary>
    public string AccessKey { get; init; } = "";

    /// <summary>The secret key.</summary>
    public string SecretKey { get; init; } = "";

    /// <summary>The bucket all objects are stored in.</summary>
    public string BucketName { get; init; } = "flowboard";

    /// <summary>Whether to use path-style addressing (required by MinIO).</summary>
    public bool UsePathStyle { get; init; } = true;
}
