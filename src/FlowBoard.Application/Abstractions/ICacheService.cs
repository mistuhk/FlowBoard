namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Abstraction for distributed cache operations (backed by Redis).
/// Cache keys must be namespaced by tenant: <c>{orgId}:{resource}:{id}</c>.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value, or <c>null</c> if the key does not exist or has expired.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically retrieves a cached value and removes it, returning <c>null</c> if the key did
    /// not exist. The read and delete happen as one operation, so a value can be consumed exactly
    /// once even under concurrent callers. Used for single-use and rotating tokens.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key to consume.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<T?> GetAndRemoveAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Stores a value in the cache with an optional expiry.</summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="expiry">Optional time-to-live. If <c>null</c>, the entry does not expire.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>Removes a single entry from the cache.</summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries whose keys begin with the given prefix.
    /// Used to invalidate all cached data for a specific resource type or tenant.
    /// </summary>
    /// <param name="prefix">The key prefix to match.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
