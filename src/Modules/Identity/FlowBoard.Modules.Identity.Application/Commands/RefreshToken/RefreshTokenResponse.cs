namespace FlowBoard.Modules.Identity.Application.Commands.RefreshToken;

/// <summary>
/// The result of a successful refresh. The new refresh token is returned to the controller so it
/// can replace the httpOnly cookie; it is never written into the JSON response body.
/// </summary>
/// <param name="AccessToken">The newly issued JWT access token.</param>
/// <param name="AccessTokenExpiresAtUtc">The UTC instant at which the access token expires.</param>
/// <param name="RefreshToken">The new opaque refresh token to be set as an httpOnly cookie.</param>
public sealed record RefreshTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken);
