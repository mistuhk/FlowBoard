using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Commands.VerifyEmail;

/// <summary>
/// Handles <see cref="VerifyEmailCommand"/>: resolves the token to a user, marks the
/// account verified, and consumes the token so it cannot be reused.
/// </summary>
public sealed class VerifyEmailCommandHandler(
    IUserRepository users,
    ICacheService cache)
    : IRequestHandler<VerifyEmailCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenKey = EmailVerificationTokens.TokenKey(request.Token);

        var userIdValue = await cache.GetAsync<string>(tokenKey, cancellationToken);
        if (userIdValue is null || !Guid.TryParse(userIdValue, out var guid))
            return Result.Failure(IdentityErrors.InvalidVerificationToken);

        var userId = UserId.From(guid);
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(IdentityErrors.InvalidVerificationToken);

        user.VerifyEmail();

        // Single-use: drop both the token lookup and the pending entry.
        await cache.RemoveAsync(tokenKey, cancellationToken);
        await cache.RemoveAsync(EmailVerificationTokens.PendingKey(userId), cancellationToken);

        return Result.Success();
    }
}
