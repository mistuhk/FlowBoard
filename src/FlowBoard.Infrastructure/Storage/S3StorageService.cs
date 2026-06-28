using FlowBoard.Application.Abstractions;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace FlowBoard.Infrastructure.Storage;

/// <summary>
/// S3-compatible object storage via the MinIO client. Issues pre-signed URLs so file bytes never
/// pass through the API; the API only coordinates upload and download URLs and object lifecycle.
/// </summary>
internal sealed class S3StorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private int _bucketEnsured;

    public S3StorageService(IOptions<StorageOptions> options)
    {
        var settings = options.Value;
        var endpoint = new Uri(settings.Endpoint);

        _client = new MinioClient()
            .WithEndpoint(endpoint.Host, endpoint.Port)
            .WithCredentials(settings.AccessKey, settings.SecretKey)
            .WithSSL(endpoint.Scheme == Uri.UriSchemeHttps)
            .Build();

        _bucket = settings.BucketName;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateUploadUrlAsync(string storageKey, string contentType, TimeSpan expiry)
    {
        await EnsureBucketAsync();
        return await _client.PresignedPutObjectAsync(new PresignedPutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithExpiry((int)expiry.TotalSeconds));
    }

    /// <inheritdoc/>
    public async Task<string> GenerateDownloadUrlAsync(string storageKey, TimeSpan expiry) =>
        await _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithExpiry((int)expiry.TotalSeconds));

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucket)
                .WithObject(storageKey), cancellationToken);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default) =>
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey), cancellationToken);

    private async Task EnsureBucketAsync()
    {
        if (Interlocked.CompareExchange(ref _bucketEnsured, 1, 0) != 0)
            return;

        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
        if (!exists)
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
    }
}
