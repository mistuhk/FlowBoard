using FlowBoard.Modules.Identity.Domain.Abstractions;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using Isopoh.Cryptography.Argon2;

namespace FlowBoard.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Argon2id implementation of <see cref="IPasswordHasher"/>.
/// Argon2id is the OWASP-recommended password hashing algorithm,
/// resistant to both GPU and side-channel attacks.
/// </summary>
internal sealed class Argon2PasswordHasher : IPasswordHasher
{
    /// <inheritdoc/>
    public HashedPassword Hash(string plaintext)
    {
        // Argon2.Hash returns an encoded string containing the algorithm
        // parameters and salt, so no separate salt storage is required.
        var encoded = Argon2.Hash(plaintext);
        return HashedPassword.FromHash(encoded);
    }

    /// <inheritdoc/>
    public bool Verify(string plaintext, HashedPassword hashedPassword)
    {
        // Argon2.Verify(encodedHash, password) reads the parameters and salt
        // from the encoded hash itself.
        return Argon2.Verify(hashedPassword.Hash, plaintext);
    }
}
