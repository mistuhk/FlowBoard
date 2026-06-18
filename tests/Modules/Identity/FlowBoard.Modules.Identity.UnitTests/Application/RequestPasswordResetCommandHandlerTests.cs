using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Commands.RequestPasswordReset;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Events;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class RequestPasswordResetCommandHandlerTests
{
    private const string TestEmail = "ada@example.com";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICacheService> _cache = new();

    private RequestPasswordResetCommandHandler CreateHandler() => new(_users.Object, _cache.Object);

    private static User RegisterUser()
    {
        var user = User.Register(
            Email.Create(TestEmail),
            HashedPassword.FromHash("$argon2id$hash"),
            DisplayName.Create("Ada Lovelace"));
        user.ClearDomainEvents();
        return user;
    }

    private static User VerifiedUser()
    {
        var user = RegisterUser();
        user.VerifyEmail();
        user.ClearDomainEvents();
        return user;
    }

    [Fact]
    public async Task Handle_issues_a_token_and_raises_the_event_for_a_verified_account()
    {
        var user = VerifiedUser();
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateHandler().Handle(new RequestPasswordResetCommand(TestEmail), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PasswordResetRequestedEvent>();
        _cache.Verify(
            c => c.SetAsync(It.Is<string>(k => k.StartsWith("password_reset:token:")), user.Id.Value.ToString(), PasswordResetTokens.Ttl, It.IsAny<CancellationToken>()),
            Times.Once);
        _cache.Verify(
            c => c.SetAsync(PasswordResetTokens.PendingKey(user.Id), It.IsAny<string>(), PasswordResetTokens.Ttl, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_succeeds_but_does_nothing_for_an_unknown_email()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new RequestPasswordResetCommand(TestEmail), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_succeeds_but_does_nothing_for_an_unverified_account()
    {
        var unverified = RegisterUser();
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unverified);

        var result = await CreateHandler().Handle(new RequestPasswordResetCommand(TestEmail), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        unverified.DomainEvents.Should().BeEmpty();
        _cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
