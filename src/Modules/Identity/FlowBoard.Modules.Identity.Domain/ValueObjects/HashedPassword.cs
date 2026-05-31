using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Identity.Domain.ValueObjects;

/// <summary>
/// Value object that encapsulates an already-computed password hash.
/// <para>
/// This type holds a hash produced elsewhere, it performs neither hashing nor
/// verification. Those responsibilities belong to an <c>IPasswordHasher</c>
/// implementation in the Infrastructure layer, keeping the Domain free of any
/// cryptographic library dependency. The hash algorithm is Argon2id.
/// </para>
/// </summary>
public sealed class HashedPassword : ValueObject
{
    private HashedPassword(string hash) => Hash = hash;

    /// <summary>The stored password hash. Never exposed to API callers.</summary>
    public string Hash { get; }

    /// <summary>
    /// Wraps an already-computed hash in a <see cref="HashedPassword"/>.
    /// Called from the Application layer with the output of
    /// <c>IPasswordHasher.Hash</c>, and from EF Core when materialising a stored hash.
    /// Not to be called with a plaintext password.
    /// </summary>
    /// <param name="encodedHash">The encoded hash string, including algorithm parameters and salt.</param>
    /// <returns>A <see cref="HashedPassword"/> wrapping the hash.</returns>
    /// <exception cref="ArgumentException">Thrown when the hash is null or whitespace.</exception>
    public static HashedPassword FromHash(string encodedHash) =>
        string.IsNullOrWhiteSpace(encodedHash)
            ? throw new ArgumentException("Hash cannot be empty.", nameof(encodedHash))
            : new HashedPassword(encodedHash);

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hash;
    }

    /// <summary>Returns a redacted representation — never the actual hash.</summary>
    public override string ToString() => "[REDACTED]";
}
