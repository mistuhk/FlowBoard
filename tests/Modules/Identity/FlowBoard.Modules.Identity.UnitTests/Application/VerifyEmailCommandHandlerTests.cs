using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Commands.VerifyEmail;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class VerifyEmailCommandHandlerTests
{
    private const string Token = "a-verification-token";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICacheService> _cache = new();

    private VerifyEmailCommandHandler CreateHandler() => new(_users.Object, _cache.Object);

    private static User RegisterUser() =>
        User.Register(
            Email.Create("ada@example.com"),
            HashedPassword.FromHash("$argon2id$hash"),
            DisplayName.Create("Ada Lovelace"));

    [Fact]
    public async Task Handle_verifies_the_user_and_consumes_both_token_entries_on_a_valid_token()
    {
        var user = RegisterUser();
        _cache.Setup(c => c.GetAsync<string>(EmailVerificationTokens.TokenKey(Token), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.Id.Value.ToString());
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateHandler().Handle(new VerifyEmailCommand(Token), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
        _cache.Verify(c => c.RemoveAsync(EmailVerificationTokens.TokenKey(Token), It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync(EmailVerificationTokens.PendingKey(user.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_fails_and_loads_no_user_when_the_token_is_unknown()
    {
        _cache.Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await CreateHandler().Handle(new VerifyEmailCommand(Token), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidVerificationToken);
        _users.Verify(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_fails_when_the_token_value_is_not_a_valid_guid()
    {
        _cache.Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("not-a-guid");

        var result = await CreateHandler().Handle(new VerifyEmailCommand(Token), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidVerificationToken);
        _users.Verify(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_fails_when_the_token_maps_to_a_missing_user()
    {
        var user = RegisterUser();
        _cache.Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.Id.Value.ToString());
        _users.Setup(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new VerifyEmailCommand(Token), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.InvalidVerificationToken);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
