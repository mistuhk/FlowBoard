using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application.Commands.Logout;

/// <summary>
/// Logs the current session out: blocklists the access token so it stops working immediately and
/// revokes the refresh token so it cannot be used to mint new tokens.
/// </summary>
/// <param name="TokenId">The access token's unique identifier (its <c>jti</c> claim).</param>
/// <param name="AccessTokenExpiresAtUtc">
/// When the access token expires. The blocklist entry lives only until this instant, after which
/// the token is invalid anyway and the entry self-cleans.
/// </param>
/// <param name="RefreshToken">
/// The refresh token from the client's cookie, or <c>null</c> if none was sent. When present it is
/// deleted so it can no longer be redeemed.
/// </param>
public sealed record LogoutCommand(
    string TokenId,
    DateTime AccessTokenExpiresAtUtc,
    string? RefreshToken) : ICommand<Result>;
