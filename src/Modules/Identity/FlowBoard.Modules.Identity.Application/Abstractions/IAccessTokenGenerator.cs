using FlowBoard.Modules.Identity.Domain.Aggregates;

namespace FlowBoard.Modules.Identity.Application.Abstractions;

/// <summary>
/// Issues signed JSON Web Token access tokens for authenticated users. Implemented in the
/// Infrastructure layer (RS256, signed with the application's RSA key) so the Application
/// layer carries no dependency on a JWT library.
/// </summary>
public interface IAccessTokenGenerator
{
    /// <summary>
    /// Generates a short-lived access token carrying the user's id (<c>sub</c>), email, and a
    /// unique token id (<c>jti</c>).
    /// </summary>
    /// <param name="user">The authenticated user the token represents.</param>
    /// <returns>The encoded token together with its <c>jti</c> and absolute expiry.</returns>
    AccessToken Generate(User user);
}

/// <summary>An issued access token and its metadata.</summary>
/// <param name="Token">The encoded, signed JWT string.</param>
/// <param name="TokenId">The token's unique identifier (the <c>jti</c> claim).</param>
/// <param name="ExpiresAtUtc">The UTC instant at which the token expires.</param>
public sealed record AccessToken(string Token, string TokenId, DateTime ExpiresAtUtc);
