using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Domain.Abstractions;
using FlowBoard.Modules.Identity.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Commands.ResetPassword;

/// <summary>
/// Handles <see cref="ResetPasswordCommand"/>: consumes the single-use reset token, sets the new
/// password (raising <c>PasswordChangedEvent</c>), and revokes every existing refresh token by
/// bumping the user's token version, so any session opened before the reset can no longer be
/// refreshed.
/// </summary>
public sealed class ResetPasswordCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ICacheService cache)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Single-use: consuming the token atomically prevents it being replayed.
        var userIdValue = await cache.GetAndRemoveAsync<string>(
            PasswordResetTokens.TokenKey(request.Token), cancellationToken);

        if (userIdValue is null || !Guid.TryParse(userIdValue, out var guid))
            return Result.Failure(IdentityErrors.InvalidPasswordResetToken);

        var userId = UserId.From(guid);
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(IdentityErrors.InvalidPasswordResetToken);

        var newPassword = passwordHasher.Hash(request.NewPassword);
        user.ChangePassword(newPassword);

        // Drop the pending entry too, and revoke all refresh tokens by bumping the token version.
        await cache.RemoveAsync(PasswordResetTokens.PendingKey(userId), cancellationToken);

        var currentVersion = await cache.GetAsync<int>(RefreshTokens.VersionKey(userId), cancellationToken);
        await cache.SetAsync(
            RefreshTokens.VersionKey(userId),
            currentVersion + 1,
            RefreshTokens.Ttl,
            cancellationToken);

        return Result.Success();
    }
}
