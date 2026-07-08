using System.Security.Cryptography;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Identity.Application;

/// <summary>
/// Generates email-verification tokens and builds the Redis keys under which they are stored.
/// <para>
/// Two entries are written on registration, both with a 24-hour time-to-live:
/// <list type="bullet">
///   <item><see cref="TokenKey"/> (token -> user id): consumed by the verify-email flow
///         to resolve and confirm the account; single-use.</item>
///   <item><see cref="PendingKey"/> (user id -> token): consumed by the
///         <c>UserRegisteredEvent</c> handler to build the verification link for the email.</item>
/// </list>
/// </para>
/// </summary>
public static class EmailVerificationTokens
{
    /// <summary>Time-to-live for a verification token. Tokens expire 24 hours after issue.</summary>
    public static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    /// <summary>Builds the Redis key mapping a verification token to its user id.</summary>
    /// <param name="token">The opaque verification token.</param>
    public static string TokenKey(string token) => $"email_verification:token:{token}";

    /// <summary>Builds the Redis key mapping a user id to its pending verification token.</summary>
    /// <param name="userId">The user awaiting email verification.</param>
    public static string PendingKey(UserId userId) => $"email_verification:pending:{userId.Value}";

    /// <summary>
    /// Generates a cryptographically-random, URL-safe verification token (256 bits of entropy).
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
