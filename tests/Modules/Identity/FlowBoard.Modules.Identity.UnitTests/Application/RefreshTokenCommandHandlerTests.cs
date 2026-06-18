using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Abstractions;
using FlowBoard.Modules.Identity.Application.Commands.RefreshToken;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class RefreshTokenCommandHandlerTests
{
    private const string OldToken = "old-refresh-token";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IAccessTokenGenerator> _accessTokens = new();
    private readonly Mock<ICacheService> _cache = new();

    private RefreshTokenCommandHandler CreateHandler() =>
        new(_users.Object, _accessTokens.Object, _cache.Object);

    private static User SomeUser() =>
        User.Register(
            Email.Create("ada@example.com"),
            HashedPassword.FromHash("$argon2id$hash"),
            DisplayName.Create("Ada Lovelace"));

    private void GivenStoredToken(User user, int version) =>
        _cache.Setup(c => c.GetAndRemoveAsync<RefreshTokenEntry>(RefreshTokens.Key(OldToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenEntry(user.Id.Value.ToString(), version));

    private void GivenCurrentVersion(User user, int version) =>
        _cache.Setup(c => c.GetAsync<int>(RefreshTokens.VersionKey(user.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

    [Fact]
    public async Task Handle_consumes_the_old_token_issues_a_new_pair_and_stores_the_new_refresh_token()
    {
        var user = SomeUser();
        GivenStoredToken(user, version: 0);
        GivenCurrentVersion(user, version: 0);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _accessTokens.Setup(g => g.Generate(user))
            .Returns(new AccessToken("new-access-token", "new-jti", DateTime.UtcNow.AddMinutes(15)));

        var result = await CreateHandler().Handle(new RefreshTokenCommand(OldToken), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().NotBeNullOrEmpty().And.NotBe(OldToken);
        _cache.Verify(
            c => c.SetAsync(
                RefreshTokens.Key(result.Value.RefreshToken),
                It.Is<RefreshTokenEntry>(e => e.UserId == user.Id.Value.ToString() && e.Version == 0),
                RefreshTokens.Ttl,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_fails_when_the_token_is_unknown_and_issues_nothing()
    {
        _cache.Setup(c => c.GetAndRemoveAsync<RefreshTokenEntry>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenEntry?)null);

        var result = await CreateHandler().Handle(new RefreshTokenCommand(OldToken), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidRefreshToken);
        _users.Verify(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
        _accessTokens.Verify(g => g.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_fails_when_the_token_version_is_stale_because_the_password_was_reset()
    {
        var user = SomeUser();
        GivenStoredToken(user, version: 0);   // token minted at version 0
        GivenCurrentVersion(user, version: 1); // a reset has since bumped the version

        var result = await CreateHandler().Handle(new RefreshTokenCommand(OldToken), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidRefreshToken);
        _users.Verify(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
        _accessTokens.Verify(g => g.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_fails_when_the_token_value_is_not_a_valid_guid()
    {
        _cache.Setup(c => c.GetAndRemoveAsync<RefreshTokenEntry>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenEntry("not-a-guid", 0));

        var result = await CreateHandler().Handle(new RefreshTokenCommand(OldToken), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidRefreshToken);
        _accessTokens.Verify(g => g.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_fails_when_the_token_maps_to_a_missing_user()
    {
        var user = SomeUser();
        GivenStoredToken(user, version: 0);
        GivenCurrentVersion(user, version: 0);
        _users.Setup(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new RefreshTokenCommand(OldToken), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidRefreshToken);
        _accessTokens.Verify(g => g.Generate(It.IsAny<User>()), Times.Never);
    }
}
