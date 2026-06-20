using System.Security.Cryptography;
using System.Text;

namespace FlowBoard.Modules.Organisations.Application;

/// <summary>
/// Generates single-use invitation tokens and hashes them for storage.
/// <para>
/// The raw token is returned to the caller (to be delivered to the invitee) and never persisted.
/// Only its SHA-256 hash is stored, and acceptance hashes the presented token to look the
/// invitation up. The token carries 256 bits of entropy, so a plain (unsalted) hash is sufficient.
/// </para>
/// </summary>
public static class InvitationTokens
{
    /// <summary>
    /// Generates a cryptographically-random, URL-safe invitation token (256 bits of entropy).
    /// </summary>
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>Computes the lowercase hex SHA-256 hash of an invitation token.</summary>
    /// <param name="token">The raw token to hash.</param>
    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
