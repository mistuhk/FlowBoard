using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Application.Abstractions;
using FlowBoard.Modules.Identity.Domain.Abstractions;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Commands.LoginUser;

/// <summary>
/// Handles <see cref="LoginUserCommand"/>: verifies the credentials, records the login, and
/// issues an access token plus an opaque refresh token (stored in the cache with a 30-day TTL).
/// <para>
/// An unknown email and a wrong password both return <see cref="IdentityErrors.InvalidCredentials"/>
/// so the response cannot be used to enumerate registered accounts. An unverified account is
/// reported distinctly so the caller can prompt the user to confirm their email.
/// </para>
/// </summary>
public sealed class LoginUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator accessTokenGenerator,
    ICacheService cache)
    : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<LoginUserResponse>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var user = await users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
            return Result.Failure<LoginUserResponse>(IdentityErrors.InvalidCredentials);

        if (!passwordHasher.Verify(request.Password, user.Password))
            return Result.Failure<LoginUserResponse>(IdentityErrors.InvalidCredentials);

        if (!user.IsEmailVerified)
            return Result.Failure<LoginUserResponse>(IdentityErrors.EmailNotVerified);

        user.RecordLogin();

        var accessToken = accessTokenGenerator.Generate(user);

        // Opaque refresh token, stored server-side so it can be rotated and revoked. It self-expires
        // after 30 days, so a token left behind by a rolled-back transaction harmlessly lapses. The
        // token records the user's current token version so a later password reset can invalidate it.
        // A missing version key reads back as 0, the default for a user who has never had a reset.
        var version = await cache.GetAsync<int>(RefreshTokens.VersionKey(user.Id), cancellationToken);
        var refreshToken = RefreshTokens.GenerateToken();
        await cache.SetAsync(
            RefreshTokens.Key(refreshToken),
            new RefreshTokenEntry(user.Id.Value.ToString(), version),
            RefreshTokens.Ttl,
            cancellationToken);

        return Result.Success(
            new LoginUserResponse(accessToken.Token, accessToken.ExpiresAtUtc, refreshToken));
    }
}
