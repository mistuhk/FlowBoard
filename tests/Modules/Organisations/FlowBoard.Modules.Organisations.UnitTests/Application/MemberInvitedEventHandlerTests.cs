using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Application.EventHandlers;
using FlowBoard.Modules.Organisations.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class MemberInvitedEventHandlerTests
{
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<ICacheService> _cache = new();

    private MemberInvitedEventHandler CreateHandler() =>
        new(_email.Object, _cache.Object, Mock.Of<ILogger<MemberInvitedEventHandler>>());

    private static DomainEventNotification<MemberInvitedEvent> Notification(OrganisationId orgId, string email) =>
        new(new MemberInvitedEvent(orgId, email, UserId.New(), "Member"));

    [Fact]
    public async Task Handle_sends_the_invitation_email_when_the_token_is_cached()
    {
        var orgId = OrganisationId.New();
        _cache
            .Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("raw-token");
        var handler = CreateHandler();

        await handler.Handle(Notification(orgId, "invitee@example.com"), CancellationToken.None);

        _email.Verify(
            e => e.SendAsync(
                "invitee@example.com",
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains("raw-token")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_skips_sending_when_no_token_is_cached()
    {
        _cache
            .Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var handler = CreateHandler();

        await handler.Handle(Notification(OrganisationId.New(), "invitee@example.com"), CancellationToken.None);

        _email.Verify(
            e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
