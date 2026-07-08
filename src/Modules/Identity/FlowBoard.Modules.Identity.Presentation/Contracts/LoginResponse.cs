namespace FlowBoard.Modules.Identity.Presentation.Contracts;

/// <summary>
/// The response body for a successful login. The refresh token is not included here: it is
/// delivered separately as an httpOnly cookie.
/// </summary>
/// <param name="AccessToken">The signed JWT access token to send as a bearer token.</param>
/// <param name="ExpiresAtUtc">The UTC instant at which the access token expires.</param>
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
