using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Commands.ResetPassword;
using FlowBoard.Modules.Identity.Domain.Abstractions;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Events;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class ResetPasswordCommandHandlerTests
{
    private const string Token = "a-reset-token";
    private const string NewPassword = "NewPassword1";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ICacheService> _cache = new();

    private ResetPasswordCommandHandler CreateHandler() =>
        new(_users.Object, _passwordHasher.Object, _cache.Object);

    private static User SomeUser()
    {
        var user = User.Register(
            Email.Create("ada@example.com"),
            HashedPassword.FromHash("$argon2id$old"),
            DisplayName.Create("Ada Lovelace"));
        user.ClearDomainEvents();
        return user;
    }

    [Fact]
    public async Task Handle_changes_the_password_consumes_the_token_and_bumps_the_refresh_version()
    {
        var user = SomeUser();
        var newHash = HashedPassword.FromHash("$argon2id$new");
        _cache.Setup(c => c.GetAndRemoveAsync<string>(PasswordResetTokens.TokenKey(Token), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.Id.Value.ToString());
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Hash(NewPassword)).Returns(newHash);
        _cache.Setup(c => c.GetAsync<int>(RefreshTokens.VersionKey(user.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await CreateHandler().Handle(new ResetPasswordCommand(Token, NewPassword), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.Password.Should().Be(newHash);
        user.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PasswordChangedEvent>();
        // The refresh-token version is incremented, invalidating every existing refresh token.
        _cache.Verify(
            c => c.SetAsync(RefreshTokens.VersionKey(user.Id), 3, RefreshTokens.Ttl, It.IsAny<CancellationToken>()),
            Times.Once);
        // The pending entry is cleared too.
        _cache.Verify(
            c => c.RemoveAsync(PasswordResetTokens.PendingKey(user.Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_fails_when_the_token_is_unknown()
    {
        _cache.Setup(c => c.GetAndRemoveAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await CreateHandler().Handle(new ResetPasswordCommand(Token, NewPassword), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidPasswordResetToken);
        _users.Verify(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
        _passwordHasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_fails_when_the_token_maps_to_a_missing_user()
    {
        var user = SomeUser();
        _cache.Setup(c => c.GetAndRemoveAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.Id.Value.ToString());
        _users.Setup(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new ResetPasswordCommand(Token, NewPassword), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidPasswordResetToken);
        _passwordHasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }
}
