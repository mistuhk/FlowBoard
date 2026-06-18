using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application.Abstractions;
using FlowBoard.Modules.Identity.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Commands.RefreshToken;

/// <summary>
/// Handles <see cref="RefreshTokenCommand"/>: atomically consumes the presented refresh token,
/// confirms the account is still active, and issues a fresh access token together with a new
/// refresh token (rotation). The old token is gone the moment it is redeemed, so a replayed or
/// concurrent second use finds nothing and fails.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    IAccessTokenGenerator accessTokenGenerator,
    ICacheService cache)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Atomic consume: the first caller to redeem the token gets the value, any other gets null.
        var entry = await cache.GetAndRemoveAsync<RefreshTokenEntry>(
            RefreshTokens.Key(request.RefreshToken), cancellationToken);

        if (entry is null || !Guid.TryParse(entry.UserId, out var guid))
            return Result.Failure<RefreshTokenResponse>(IdentityErrors.InvalidRefreshToken);

        var userId = UserId.From(guid);

        // Reject a token whose version is stale, that is, one issued before a password reset
        // bumped the user's token version.
        var currentVersion = await cache.GetAsync<int>(RefreshTokens.VersionKey(userId), cancellationToken);
        if (entry.Version != currentVersion)
            return Result.Failure<RefreshTokenResponse>(IdentityErrors.InvalidRefreshToken);

        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure<RefreshTokenResponse>(IdentityErrors.InvalidRefreshToken);

        var accessToken = accessTokenGenerator.Generate(user);

        var newRefreshToken = RefreshTokens.GenerateToken();
        await cache.SetAsync(
            RefreshTokens.Key(newRefreshToken),
            new RefreshTokenEntry(user.Id.Value.ToString(), currentVersion),
            RefreshTokens.Ttl,
            cancellationToken);

        return Result.Success(
            new RefreshTokenResponse(accessToken.Token, accessToken.ExpiresAtUtc, newRefreshToken));
    }
}
