using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Commands.Logout;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class LogoutCommandHandlerTests
{
    private const string Jti = "the-jti";
    private const string RefreshToken = "the-refresh-token";

    private readonly Mock<ICacheService> _cache = new();

    private LogoutCommandHandler CreateHandler() => new(_cache.Object);

    [Fact]
    public async Task Handle_blocklists_the_jti_for_its_remaining_lifetime_and_removes_the_refresh_token()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var result = await CreateHandler().Handle(
            new LogoutCommand(Jti, expiresAt, RefreshToken), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _cache.Verify(
            c => c.SetAsync(
                AccessTokenBlocklist.Key(Jti),
                It.IsAny<string>(),
                It.Is<TimeSpan?>(t => t.HasValue && t.Value > TimeSpan.Zero && t.Value <= TimeSpan.FromMinutes(15)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _cache.Verify(
            c => c.RemoveAsync(RefreshTokens.Key(RefreshToken), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_does_not_blocklist_an_already_expired_token()
    {
        var expiredAt = DateTime.UtcNow.AddMinutes(-1);

        var result = await CreateHandler().Handle(
            new LogoutCommand(Jti, expiredAt, RefreshToken), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        // The refresh token is still revoked even when the access token has already lapsed.
        _cache.Verify(
            c => c.RemoveAsync(RefreshTokens.Key(RefreshToken), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_skips_the_refresh_token_removal_when_none_was_supplied()
    {
        var result = await CreateHandler().Handle(
            new LogoutCommand(Jti, DateTime.UtcNow.AddMinutes(15), null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
