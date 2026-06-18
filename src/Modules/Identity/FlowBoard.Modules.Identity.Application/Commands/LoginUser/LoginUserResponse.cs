namespace FlowBoard.Modules.Identity.Application.Commands.LoginUser;

/// <summary>
/// The result of a successful login. The refresh token is returned to the controller so it can
/// be delivered as an httpOnly cookie; it is never written into the JSON response body.
/// </summary>
/// <param name="AccessToken">The signed JWT access token.</param>
/// <param name="AccessTokenExpiresAtUtc">The UTC instant at which the access token expires.</param>
/// <param name="RefreshToken">The opaque refresh token to be set as an httpOnly cookie.</param>
public sealed record LoginUserResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken);
