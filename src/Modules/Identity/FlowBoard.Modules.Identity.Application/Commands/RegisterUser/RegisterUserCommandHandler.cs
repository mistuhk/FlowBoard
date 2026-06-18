using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Domain.Abstractions;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Commands.RegisterUser;

/// <summary>
/// Handles <see cref="RegisterUserCommand"/>: enforces email uniqueness, hashes the
/// password, creates the <see cref="User"/> aggregate, and issues a single-use email
/// verification token into the cache (24-hour TTL).
/// <para>
/// The aggregate raises <c>UserRegisteredEvent</c>, which the outbox dispatches so the
/// verification email is sent asynchronously, never inline in this handler.
/// </para>
/// </summary>
public sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ICacheService cache)
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (await users.ExistsByEmailAsync(email, cancellationToken))
            return Result.Failure<RegisterUserResponse>(IdentityErrors.EmailAlreadyInUse);

        var displayName = DisplayName.Create(request.DisplayName);
        var hashedPassword = passwordHasher.Hash(request.Password);

        var user = User.Register(email, hashedPassword, displayName);
        await users.AddAsync(user, cancellationToken);

        // Issue the verification token. Stored here (rather than in the asynchronous event
        // handler) so it is available the moment registration commits; both entries self-expire
        // after 24h, so a token left behind by a rolled-back transaction harmlessly lapses.
        var token = EmailVerificationTokens.GenerateToken();
        await cache.SetAsync(
            EmailVerificationTokens.TokenKey(token),
            user.Id.Value.ToString(),
            EmailVerificationTokens.Ttl,
            cancellationToken);

        await cache.SetAsync(
            EmailVerificationTokens.PendingKey(user.Id),
            token,
            EmailVerificationTokens.Ttl,
            cancellationToken);

        return Result.Success(
            new RegisterUserResponse(user.Id.Value, user.Email.Value, user.DisplayName.Value));
    }
}
