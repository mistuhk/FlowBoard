using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Application.Commands.CreateNotification;
using FlowBoard.Modules.Notifications.Application.EventHandlers;
using FlowBoard.Modules.Notifications.Domain.Aggregates;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Events;
using FluentAssertions;
using MediatR;
using Moq;

namespace FlowBoard.Modules.Notifications.UnitTests.Application;

public sealed class NotificationHandlerTests
{
    [Fact]
    public async Task CreateNotification_persists_a_notification()
    {
        var repo = new Mock<INotificationRepository>();
        var handler = new CreateNotificationCommandHandler(repo.Object);

        var result = await handler.Handle(
            new CreateNotificationCommand(
                Guid.NewGuid(), Guid.NewGuid(), NotificationType.TaskAssigned.DbValue, "msg", "task", Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TaskAssignedEventHandler_dispatches_a_create_notification_command_for_the_assignee()
    {
        var sender = new Mock<ISender>();
        var handler = new TaskAssignedEventHandler(sender.Object);
        var assigneeId = UserId.New();
        var orgId = OrganisationId.New();
        var taskId = TaskId.New();
        var @event = new TaskAssignedEvent(taskId, orgId, assigneeId, UserId.New());

        await handler.Handle(new DomainEventNotification<TaskAssignedEvent>(@event), CancellationToken.None);

        sender.Verify(
            s => s.Send(
                It.Is<CreateNotificationCommand>(c =>
                    c.UserId == assigneeId.Value
                    && c.OrganisationId == orgId.Value
                    && c.Type == NotificationType.TaskAssigned.DbValue
                    && c.EntityId == taskId.Value),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
