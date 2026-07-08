using System.Security.Cryptography;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Identity.Application;

/// <summary>
/// Generates opaque refresh tokens and builds the Redis keys under which each is stored.
/// <para>
/// A refresh token is a random, server-side secret (it is not a JWT and carries no claims).
/// On login one <see cref="RefreshTokenEntry"/> is written, mapping the token to its user id and
/// the user's current token version, with a 30-day time-to-live; the rotate and revoke flows
/// consume and replace it.
/// </para>
/// <para>
/// Every token records the user's token <em>version</em> at the moment it was issued. A password
/// reset bumps <see cref="VersionKey"/>, so every previously issued refresh token now carries a
/// stale version and is rejected on use. This invalidates all of a user's sessions at once
/// without an index or a scan.
/// </para>
/// </summary>
public static class RefreshTokens
{
    /// <summary>Time-to-live for a refresh token. Tokens expire 30 days after issue.</summary>
    public static readonly TimeSpan Ttl = TimeSpan.FromDays(30);

    /// <summary>Builds the Redis key mapping a refresh token to its <see cref="RefreshTokenEntry"/>.</summary>
    /// <param name="token">The opaque refresh token.</param>
    public static string Key(string token) => $"refresh_token:{token}";

    /// <summary>Builds the Redis key holding a user's current refresh-token version.</summary>
    /// <param name="userId">The user whose token version is tracked.</param>
    public static string VersionKey(UserId userId) => $"refresh_token_version:{userId.Value}";

    /// <summary>
    /// Generates a cryptographically-random, URL-safe refresh token (256 bits of entropy).
    /// </summary>
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}

/// <summary>
/// The value stored against a refresh token: the owning user and the user's token version at the
/// moment the token was issued.
/// </summary>
/// <param name="UserId">The owning user's id.</param>
/// <param name="Version">The user's refresh-token version when this token was minted.</param>
public sealed record RefreshTokenEntry(string UserId, int Version);
