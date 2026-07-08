using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace FlowBoard.Infrastructure.Authentication;

/// <summary>
/// Provides the RSA key used for RS256 access tokens. The key is loaded from
/// <see cref="JwtOptions.PrivateKeyPem"/> when configured; otherwise an ephemeral 2048-bit key is
/// generated for local development and tests. The key is held for the lifetime of the singleton,
/// so a single instance signs and validates every token.
/// </summary>
public sealed class RsaJwtKeyProvider : IJwtKeyProvider, IDisposable
{
    private readonly RSA _rsa;

    /// <summary>Creates the provider from the given options.</summary>
    /// <param name="options">The JWT options carrying the optional PEM private key.</param>
    /// <param name="logger">A logger used to warn when an ephemeral key is generated.</param>
    public RsaJwtKeyProvider(JwtOptions options, ILogger<RsaJwtKeyProvider>? logger = null)
    {
        _rsa = RSA.Create();

        if (!string.IsNullOrWhiteSpace(options.PrivateKeyPem))
        {
            _rsa.ImportFromPem(options.PrivateKeyPem);
        }
        else
        {
            _rsa.KeySize = 2048;
            (logger ?? NullLogger<RsaJwtKeyProvider>.Instance).LogWarning(
                "No Jwt:PrivateKeyPem configured. Generated an ephemeral RSA signing key. " +
                "Tokens will not survive a restart and are not valid across instances. " +
                "Configure a key from a secret for any deployed environment.");
        }

        // KeyId stays stable for the key's lifetime so issued tokens carry a consistent kid.
        SigningKey = new RsaSecurityKey(_rsa) { KeyId = Guid.NewGuid().ToString("N") };
        SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.RsaSha256);
    }

    /// <inheritdoc/>
    public RsaSecurityKey SigningKey { get; }

    /// <inheritdoc/>
    public SigningCredentials SigningCredentials { get; }

    /// <inheritdoc/>
    public void Dispose() => _rsa.Dispose();
}
