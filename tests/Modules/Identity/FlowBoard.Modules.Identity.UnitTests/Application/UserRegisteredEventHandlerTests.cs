using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application.EventHandlers;
using FlowBoard.Modules.Identity.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class UserRegisteredEventHandlerTests
{
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<ICacheService> _cache = new();

    private UserRegisteredEventHandler CreateHandler() =>
        new(_email.Object, _cache.Object, NullLogger<UserRegisteredEventHandler>.Instance);

    private static DomainEventNotification<UserRegisteredEvent> Notification() =>
        new(new UserRegisteredEvent(UserId.New(), "ada@example.com"));

    [Fact]
    public async Task Handle_sends_a_verification_email_containing_the_pending_token()
    {
        _cache.Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("the-verification-token");

        await CreateHandler().Handle(Notification(), CancellationToken.None);

        _email.Verify(e => e.SendAsync(
                "ada@example.com",
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains("the-verification-token")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_does_not_send_an_email_when_no_pending_token_exists()
    {
        _cache.Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        await CreateHandler().Handle(Notification(), CancellationToken.None);

        _email.Verify(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
