using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowBoard.Infrastructure.Authentication;
using FlowBoard.Modules.Identity.Application.Abstractions;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using Microsoft.Extensions.Options;

namespace FlowBoard.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Issues RS256-signed JWT access tokens, signed with the application's shared RSA key. Each
/// token carries the user's id (<c>sub</c>), email, and a unique token id (<c>jti</c>), and
/// expires after <see cref="JwtOptions.AccessTokenExpiryMinutes"/>.
/// </summary>
internal sealed class JwtAccessTokenGenerator(
    IJwtKeyProvider keyProvider,
    IOptions<JwtOptions> options) : IAccessTokenGenerator
{
    private readonly JwtSecurityTokenHandler _handler = new();

    /// <inheritdoc/>
    public AccessToken Generate(User user)
    {
        var jwtOptions = options.Value;

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(jwtOptions.AccessTokenExpiryMinutes);
        var tokenId = Guid.NewGuid().ToString("N");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Jti, tokenId),
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: keyProvider.SigningCredentials);

        return new AccessToken(_handler.WriteToken(token), tokenId, expiresAt);
    }
}
