using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Abstractions;
using FlowBoard.Modules.Identity.Application.Commands.LoginUser;
using FlowBoard.Modules.Identity.Domain.Abstractions;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class LoginUserCommandHandlerTests
{
    private const string TestEmail = "ada@example.com";
    private const string Password = "Password1";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IAccessTokenGenerator> _accessTokens = new();
    private readonly Mock<ICacheService> _cache = new();

    private LoginUserCommandHandler CreateHandler() =>
        new(_users.Object, _passwordHasher.Object, _accessTokens.Object, _cache.Object);

    private static User VerifiedUser()
    {
        var user = User.Register(
            Email.Create(TestEmail),
            HashedPassword.FromHash("$argon2id$hash"),
            DisplayName.Create("Ada Lovelace"));
        user.VerifyEmail();
        return user;
    }

    private void GivenUserExists(User user) =>
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    private void GivenPasswordVerifies(bool verifies) =>
        _passwordHasher.Setup(h => h.Verify(Password, It.IsAny<HashedPassword>()))
            .Returns(verifies);

    [Fact]
    public async Task Handle_returns_tokens_records_the_login_and_stores_the_refresh_token_on_valid_credentials()
    {
        var user = VerifiedUser();
        GivenUserExists(user);
        GivenPasswordVerifies(true);
        _accessTokens.Setup(g => g.Generate(user))
            .Returns(new AccessToken("the-access-token", "the-jti", DateTime.UtcNow.AddMinutes(15)));

        var result = await CreateHandler().Handle(new LoginUserCommand(TestEmail, Password), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("the-access-token");
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        user.LastLoginAt.Should().NotBeNull();
        _cache.Verify(
            c => c.SetAsync(
                RefreshTokens.Key(result.Value.RefreshToken),
                It.Is<RefreshTokenEntry>(e => e.UserId == user.Id.Value.ToString() && e.Version == 0),
                RefreshTokens.Ttl,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_fails_with_invalid_credentials_when_the_email_is_unknown()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new LoginUserCommand(TestEmail, Password), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
        _accessTokens.Verify(g => g.Generate(It.IsAny<User>()), Times.Never);
        _cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_fails_with_invalid_credentials_when_the_password_is_wrong()
    {
        GivenUserExists(VerifiedUser());
        GivenPasswordVerifies(false);

        var result = await CreateHandler().Handle(new LoginUserCommand(TestEmail, Password), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidCredentials);
        _accessTokens.Verify(g => g.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_fails_with_email_not_verified_when_the_account_is_unverified()
    {
        var unverified = User.Register(
            Email.Create(TestEmail),
            HashedPassword.FromHash("$argon2id$hash"),
            DisplayName.Create("Ada Lovelace"));
        GivenUserExists(unverified);
        GivenPasswordVerifies(true);

        var result = await CreateHandler().Handle(new LoginUserCommand(TestEmail, Password), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.EmailNotVerified);
        unverified.LastLoginAt.Should().BeNull();
        _accessTokens.Verify(g => g.Generate(It.IsAny<User>()), Times.Never);
    }
}
