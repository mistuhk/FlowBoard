namespace FlowBoard.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed binding of the <c>Jwt</c> configuration section. Drives both token issuance
/// (Identity) and bearer validation (the API composition root).
/// </summary>
public sealed class JwtOptions
{
    /// <summary>The configuration section name these options bind to.</summary>
    public const string SectionName = "Jwt";

    /// <summary>The token issuer (<c>iss</c> claim), validated on every request.</summary>
    public string Issuer { get; init; } = "flowboard";

    /// <summary>The intended audience (<c>aud</c> claim), validated on every request.</summary>
    public string Audience { get; init; } = "flowboard-api";

    /// <summary>Lifetime of an access token in minutes.</summary>
    public int AccessTokenExpiryMinutes { get; init; } = 15;

    /// <summary>Lifetime of a refresh token in days.</summary>
    public int RefreshTokenExpiryDays { get; init; } = 30;

    /// <summary>
    /// The RSA private key in PEM format used to sign access tokens (RS256). Supplied from a
    /// secret or environment variable in deployed environments. When left empty the key provider
    /// generates an ephemeral key for local development and tests; such tokens do not survive a
    /// restart and are not valid across instances.
    /// </summary>
    public string? PrivateKeyPem { get; init; }
}
