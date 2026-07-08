using FlowBoard.Application.Abstractions;
using StackExchange.Redis;
using System.Text.Json;

namespace FlowBoard.Infrastructure.Caching;

/// <summary>
/// Redis-backed implementation of <see cref="ICacheService"/> using StackExchange.Redis.
/// Cache keys must be namespaced by tenant: <c>{orgId}:{resource}:{id}</c>.
/// </summary>
public sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;

        // RedisValue must be explicitly cast to string before deserialisation.
        var json = (string)value!;
        return JsonSerializer.Deserialize<T>(json);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAndRemoveAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // StringGetDeleteAsync reads and deletes in a single atomic Redis operation.
        var value = await _db.StringGetDeleteAsync(key);
        if (value.IsNullOrEmpty) return default;

        var json = (string)value!;
        return JsonSerializer.Deserialize<T>(json);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var serialised = JsonSerializer.Serialize(value);

        if (expiry.HasValue)
            await _db.StringSetAsync(key, serialised, expiry.Value);
        else
            await _db.StringSetAsync(key, serialised);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Expression-bodied async methods require the async keyword on the method,
        // not just on the lambda. Use a block body to avoid compiler error CS4010.
        await _db.KeyDeleteAsync(key);
    }

    /// <inheritdoc/>
    public Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Full implementation requires a Redis SCAN loop across all nodes.
        // Implement per-deployment once the Redis topology is confirmed.
        throw new NotImplementedException(
            "Prefix-based cache invalidation requires SCAN — implement per-deployment.");
    }
}
