using FlowBoard.Modules.Identity.Domain.ValueObjects;

namespace FlowBoard.Modules.Identity.Domain.Abstractions;

/// <summary>
/// Hashes and verifies passwords. Implemented in the Infrastructure layer
/// so the Domain carries no dependency on a cryptographic library.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes a plaintext password using Argon2id.</summary>
    HashedPassword Hash(string plaintext);

    /// <summary>Verifies a plaintext password against a stored hash.</summary>
    bool Verify(string plaintext, HashedPassword hashedPassword);
}
