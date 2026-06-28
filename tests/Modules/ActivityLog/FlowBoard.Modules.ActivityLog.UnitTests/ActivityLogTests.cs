using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.ActivityLog.Application.Commands.LogActivity;
using FlowBoard.Modules.ActivityLog.Application.EventHandlers;
using FlowBoard.Modules.ActivityLog.Domain;
using FlowBoard.Modules.Tasks.Domain.Events;
using FluentAssertions;
using MediatR;
using Moq;

namespace FlowBoard.Modules.ActivityLog.UnitTests;

public sealed class ActivityLogTests
{
    [Fact]
    public void Create_sets_the_fields_and_stamps_created_at()
    {
        var orgId = OrganisationId.New();
        var actor = UserId.New();
        var entityId = Guid.NewGuid();

        var entry = ActivityLogEntry.Create(orgId, "Task", entityId, "task.created", actor,
            new Dictionary<string, object> { ["k"] = "v" });

        entry.OrganisationId.Should().Be(orgId);
        entry.EntityType.Should().Be("Task");
        entry.EntityId.Should().Be(entityId);
        entry.EventType.Should().Be("task.created");
        entry.ActorId.Should().Be(actor);
        entry.Metadata.Should().ContainKey("k");
        entry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task TaskAssignedActivityHandler_maps_to_a_task_assigned_log_command()
    {
        var sender = new Mock<ISender>();
        var handler = new TaskAssignedActivityHandler(sender.Object);
        var taskId = TaskId.New();
        var orgId = OrganisationId.New();
        var assignee = UserId.New();
        var assignedBy = UserId.New();
        var @event = new TaskAssignedEvent(taskId, orgId, assignee, assignedBy);

        await handler.Handle(new DomainEventNotification<TaskAssignedEvent>(@event), CancellationToken.None);

        sender.Verify(s => s.Send(
            It.Is<LogActivityCommand>(c =>
                c.OrganisationId == orgId.Value
                && c.EntityType == "Task"
                && c.EntityId == taskId.Value
                && c.EventType == "task.assigned"
                && c.ActorId == assignedBy.Value),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogActivityCommandHandler_appends_an_entry()
    {
        var repo = new Mock<IActivityLogRepository>();
        var handler = new LogActivityCommandHandler(repo.Object);

        var result = await handler.Handle(
            new LogActivityCommand(Guid.NewGuid(), "Project", Guid.NewGuid(), "project.created", Guid.NewGuid(), null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.AddAsync(It.IsAny<ActivityLogEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
