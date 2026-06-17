using Microsoft.IdentityModel.Tokens;

namespace FlowBoard.Infrastructure.Authentication;

/// <summary>
/// Supplies the RSA key material used to sign and validate access tokens. Registered as a
/// singleton so issuance (Identity) and bearer validation (the API) share one key.
/// </summary>
public interface IJwtKeyProvider
{
    /// <summary>The RSA security key. Holds the private key, so it both signs and validates.</summary>
    RsaSecurityKey SigningKey { get; }

    /// <summary>RS256 signing credentials built from <see cref="SigningKey"/>.</summary>
    SigningCredentials SigningCredentials { get; }
}
