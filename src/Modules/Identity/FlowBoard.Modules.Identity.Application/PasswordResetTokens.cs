using System.Security.Cryptography;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Identity.Application;

/// <summary>
/// Generates password-reset tokens and builds the Redis keys under which they are stored.
/// <para>
/// Two entries are written when a reset is requested for a known account, both with a one-hour
/// time-to-live:
/// <list type="bullet">
///   <item><see cref="TokenKey"/> (token -> user id): consumed by the reset flow to resolve the
///         account; single-use.</item>
///   <item><see cref="PendingKey"/> (user id -> token): consumed by the
///         <c>PasswordResetRequestedEvent</c> handler to build the reset link for the email.</item>
/// </list>
/// </para>
/// </summary>
public static class PasswordResetTokens
{
    /// <summary>Time-to-live for a password-reset token. Tokens expire one hour after issue.</summary>
    public static readonly TimeSpan Ttl = TimeSpan.FromHours(1);

    /// <summary>Builds the Redis key mapping a reset token to its user id.</summary>
    /// <param name="token">The opaque reset token.</param>
    public static string TokenKey(string token) => $"password_reset:token:{token}";

    /// <summary>Builds the Redis key mapping a user id to its pending reset token.</summary>
    /// <param name="userId">The user who requested the reset.</param>
    public static string PendingKey(UserId userId) => $"password_reset:pending:{userId.Value}";

    /// <summary>
    /// Generates a cryptographically-random, URL-safe reset token (256 bits of entropy).
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
