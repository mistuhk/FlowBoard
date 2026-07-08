using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application.Commands.RefreshToken;

/// <summary>
/// Exchanges a valid refresh token for a new access token and a new refresh token. The presented
/// refresh token is consumed (rotated), so it cannot be used again.
/// </summary>
/// <param name="RefreshToken">The opaque refresh token presented by the client (from its cookie).</param>
public sealed record RefreshTokenCommand(string RefreshToken)
    : ICommand<Result<RefreshTokenResponse>>;
