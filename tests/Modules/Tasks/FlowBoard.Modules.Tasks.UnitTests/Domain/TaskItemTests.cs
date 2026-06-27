using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Events;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Tasks.UnitTests.Domain;

public sealed class TaskItemTests
{
    private static readonly ProjectId ProjectId = ProjectId.New();
    private static readonly OrganisationId OrgId = OrganisationId.New();
    private static readonly UserId CreatorId = UserId.New();

    private static TaskItem Create() =>
        TaskItem.Create(ProjectId, OrgId, "Write the spec", "details", Priority.Medium, CreatorId);

    [Fact]
    public void Create_starts_in_Todo_and_raises_TaskCreatedEvent()
    {
        var task = Create();

        task.Status.Should().Be(TaskItemStatus.Todo);
        task.Priority.Should().Be(Priority.Medium);
        task.OrganisationId.Should().Be(OrgId);
        task.ProjectId.Should().Be(ProjectId);
        task.AssigneeId.Should().BeNull();
        task.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TaskCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_an_empty_title(string title)
    {
        var act = () => TaskItem.Create(ProjectId, OrgId, title, null, Priority.Low, CreatorId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_rejects_a_title_longer_than_255_characters()
    {
        var act = () => TaskItem.Create(ProjectId, OrgId, new string('a', 256), null, Priority.Low, CreatorId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeStatus_applies_a_valid_transition_and_raises_an_event()
    {
        var task = Create();
        task.ClearDomainEvents();

        task.ChangeStatus(TaskItemStatus.InProgress, CreatorId);

        task.Status.Should().Be(TaskItemStatus.InProgress);
        var @event = task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskStatusChangedEvent>().Subject;
        @event.OldStatus.Should().Be("Todo");
        @event.NewStatus.Should().Be("InProgress");
    }

    [Fact]
    public void ChangeStatus_throws_on_an_invalid_transition()
    {
        var task = Create();  // Todo

        var act = () => task.ChangeStatus(TaskItemStatus.Done, CreatorId);

        act.Should().Throw<InvalidStatusTransitionException>();
        task.Status.Should().Be(TaskItemStatus.Todo);
    }

    [Fact]
    public void Assign_sets_the_assignee_and_raises_TaskAssignedEvent()
    {
        var task = Create();
        task.ClearDomainEvents();
        var assignee = UserId.New();

        task.Assign(assignee, CreatorId);

        task.AssigneeId.Should().Be(assignee);
        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskAssignedEvent>()
            .Which.AssigneeId.Should().Be(assignee);
    }

    [Fact]
    public void Unassign_clears_the_assignee_and_raises_TaskUnassignedEvent()
    {
        var task = Create();
        var assignee = UserId.New();
        task.Assign(assignee, CreatorId);
        task.ClearDomainEvents();

        task.Unassign();

        task.AssigneeId.Should().BeNull();
        task.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TaskUnassignedEvent>()
            .Which.PreviousAssigneeId.Should().Be(assignee);
    }

    [Fact]
    public void Unassign_is_a_no_op_when_already_unassigned()
    {
        var task = Create();
        task.ClearDomainEvents();

        task.Unassign();

        task.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangePriority_raises_an_event_only_when_it_changes()
    {
        var task = Create();  // Medium
        task.ClearDomainEvents();

        task.ChangePriority(Priority.Medium);
        task.DomainEvents.Should().BeEmpty();

        task.ChangePriority(Priority.High);
        task.Priority.Should().Be(Priority.High);
        task.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TaskPriorityChangedEvent>();
    }

    [Fact]
    public void Delete_soft_deletes_and_raises_TaskDeletedEvent_once()
    {
        var task = Create();
        task.ClearDomainEvents();

        task.Delete(CreatorId);
        task.DeletedAt.Should().NotBeNull();
        task.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TaskDeletedEvent>();

        task.ClearDomainEvents();
        task.Delete(CreatorId);
        task.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDetails_changes_the_editable_fields()
    {
        var task = Create();
        var due = DateTime.UtcNow.AddDays(7);

        task.UpdateDetails("New title", "new description", due);

        task.Title.Should().Be("New title");
        task.Description.Should().Be("new description");
        task.DueDate.Should().Be(due);
    }
}
