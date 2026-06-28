using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Application.Commands.CreateNotification;
using FlowBoard.Modules.Notifications.Application.EventHandlers;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Events;
using MediatR;
using Moq;

namespace FlowBoard.Modules.Notifications.UnitTests.Application;

public sealed class UserMentionedEventHandlerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<IUserDirectory> _directory = new();
    private readonly OrganisationId _orgId = OrganisationId.New();
    private readonly UserId _author = UserId.New();

    private Task Handle(string handle)
    {
        var handler = new UserMentionedEventHandler(_sender.Object, _directory.Object);
        var @event = new UserMentionedEvent(CommentId.New(), TaskId.New(), _orgId, _author, handle);
        return handler.Handle(new DomainEventNotification<UserMentionedEvent>(@event), CancellationToken.None);
    }

    [Fact]
    public async Task Creates_a_user_mentioned_notification_for_a_resolved_member()
    {
        var mentioned = Guid.NewGuid();
        _directory.Setup(d => d.ResolveHandleAsync(_orgId, "grace", It.IsAny<CancellationToken>())).ReturnsAsync(mentioned);

        await Handle("grace");

        _sender.Verify(
            s => s.Send(
                It.Is<CreateNotificationCommand>(c =>
                    c.UserId == mentioned
                    && c.OrganisationId == _orgId.Value
                    && c.Type == NotificationType.UserMentioned.DbValue),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Ignores_a_handle_that_resolves_to_no_member()
    {
        _directory.Setup(d => d.ResolveHandleAsync(_orgId, "nobody", It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);

        await Handle("nobody");

        _sender.Verify(s => s.Send(It.IsAny<CreateNotificationCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Does_not_notify_the_author_of_a_self_mention()
    {
        _directory.Setup(d => d.ResolveHandleAsync(_orgId, "me", It.IsAny<CancellationToken>())).ReturnsAsync(_author.Value);

        await Handle("me");

        _sender.Verify(s => s.Send(It.IsAny<CreateNotificationCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
