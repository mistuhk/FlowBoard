using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Commands.Logout;

/// <summary>
/// Handles <see cref="LogoutCommand"/>: adds the access token's <c>jti</c> to the blocklist for the
/// remainder of its lifetime and deletes the refresh token. Idempotent, so logging out twice (or
/// logging out a session whose refresh token is already gone) still succeeds.
/// </summary>
public sealed class LogoutCommandHandler(ICacheService cache)
    : IRequestHandler<LogoutCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var remainingLifetime = request.AccessTokenExpiresAtUtc - DateTime.UtcNow;
        if (remainingLifetime > TimeSpan.Zero)
        {
            await cache.SetAsync(
                AccessTokenBlocklist.Key(request.TokenId),
                "revoked",
                remainingLifetime,
                cancellationToken);
        }

        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            await cache.RemoveAsync(RefreshTokens.Key(request.RefreshToken), cancellationToken);
        }

        return Result.Success();
    }
}
