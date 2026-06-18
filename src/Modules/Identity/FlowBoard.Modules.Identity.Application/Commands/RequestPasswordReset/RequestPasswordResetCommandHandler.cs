using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Commands.RequestPasswordReset;

/// <summary>
/// Handles <see cref="RequestPasswordResetCommand"/>: when the email belongs to a verified account,
/// issues a single-use reset token (one-hour TTL) and raises
/// <c>PasswordResetRequestedEvent</c> so the reset email is sent from the outbox.
/// <para>
/// The result is always success and the work is identical in shape whether or not the account
/// exists, so the response cannot be used to discover which emails are registered.
/// </para>
/// </summary>
public sealed class RequestPasswordResetCommandHandler(
    IUserRepository users,
    ICacheService cache)
    : IRequestHandler<RequestPasswordResetCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var user = await users.GetByEmailAsync(email, cancellationToken);

        // Only verified, active accounts receive a reset link. Unknown or unverified addresses are
        // silently ignored so the caller learns nothing about which emails exist.
        if (user is not null && user.IsEmailVerified)
        {
            var token = PasswordResetTokens.GenerateToken();

            await cache.SetAsync(
                PasswordResetTokens.TokenKey(token),
                user.Id.Value.ToString(),
                PasswordResetTokens.Ttl,
                cancellationToken);

            await cache.SetAsync(
                PasswordResetTokens.PendingKey(user.Id),
                token,
                PasswordResetTokens.Ttl,
                cancellationToken);

            user.RequestPasswordReset();
        }

        return Result.Success();
    }
}
