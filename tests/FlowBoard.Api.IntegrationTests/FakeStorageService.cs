using FlowBoard.Application.Abstractions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Test double for <see cref="IStorageService"/>. The integration environment has no MinIO, so this
/// returns canned pre-signed URLs and treats every object as present, exercising the coordination
/// flow (request -> confirm -> download -> remove) without real object storage.
/// </summary>
internal sealed class FakeStorageService : IStorageService
{
    public Task<string> GenerateUploadUrlAsync(string storageKey, string contentType, TimeSpan expiry) =>
        Task.FromResult($"https://storage.test/upload/{storageKey}");

    public Task<string> GenerateDownloadUrlAsync(string storageKey, TimeSpan expiry) =>
        Task.FromResult($"https://storage.test/download/{storageKey}");

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
