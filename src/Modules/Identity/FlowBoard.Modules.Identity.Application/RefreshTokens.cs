using System.Security.Cryptography;

namespace FlowBoard.Modules.Identity.Application;

/// <summary>
/// Generates opaque refresh tokens and builds the Redis key under which each is stored.
/// <para>
/// A refresh token is a random, server-side secret (it is not a JWT and carries no claims).
/// On login one entry is written, mapping the token to its user id with a 30-day time-to-live;
/// the verify, rotate, and revoke flows in later stories consume and replace it.
/// </para>
/// </summary>
public static class RefreshTokens
{
    /// <summary>Time-to-live for a refresh token. Tokens expire 30 days after issue.</summary>
    public static readonly TimeSpan Ttl = TimeSpan.FromDays(30);

    /// <summary>Builds the Redis key mapping a refresh token to its user id.</summary>
    /// <param name="token">The opaque refresh token.</param>
    public static string Key(string token) => $"refresh_token:{token}";

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
