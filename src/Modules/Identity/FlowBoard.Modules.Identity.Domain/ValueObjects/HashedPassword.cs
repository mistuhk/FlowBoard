using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates a hashed password.
/// Plaintext passwords never leave this class, only the hash is stored.
/// The hash algorithm is Argon2id.
/// </summary>
public sealed class HashedPassword : ValueObject
{
    private const int MinLength = 8;

    private HashedPassword(string hash) => Hash = hash;

    /// <summary>The stored password hash. Never exposed to API callers.</summary>
    public string Hash { get; }

    /// <summary>
    /// Validates and hashes a plaintext password.
    /// </summary>
    /// <param name="plaintext">The plaintext password provided by the user.</param>
    /// <returns>A <see cref="HashedPassword"/> wrapping the resulting hash.</returns>
    /// <exception cref="DomainException">Thrown if the password is shorter than the minimum length.</exception>
    public static HashedPassword FromPlaintext(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext) || plaintext.Length < MinLength)
            throw new DomainException($"Password must be at least {MinLength} characters.");

        // TODO: Replace with Argon2id via BCrypt.Net-Next or Isopoh.Cryptography.Argon2
        var hash = BCryptPlaceholder.Hash(plaintext);
        return new HashedPassword(hash);
    }

    /// <summary>
    /// Reconstructs a <see cref="HashedPassword"/> from a hash already stored in the database.
    /// Used during EF Core materialisation — do not call with a plaintext value.
    /// </summary>
    /// <param name="hash">The stored hash string.</param>
    public static HashedPassword FromHash(string hash) => new(hash);

    /// <summary>
    /// Verifies a plaintext password against the stored hash.
    /// </summary>
    /// <param name="plaintext">The plaintext password to verify.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
    public bool Verify(string plaintext) =>
        BCryptPlaceholder.Verify(plaintext, Hash);

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hash;
    }

    // Placeholder implementation, NOT suitable for production.
    // I need to replace with Argon2id before MVP.
    private static class BCryptPlaceholder
    {
        public static string Hash(string input) =>
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));

        public static bool Verify(string input, string hash) =>
            Hash(input) == hash;
    }
}
