using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Aggregates;
using FlowBoard.Modules.Projects.Domain.Events;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Projects.UnitTests.Domain;

public sealed class ProjectTests
{
    private static readonly OrganisationId OrgId = OrganisationId.New();
    private static readonly UserId CreatorId = UserId.New();

    private static Project Create() =>
        Project.Create(OrgId, ProjectName.Create("Website Revamp"), "A description", CreatorId);

    [Fact]
    public void Create_makes_an_active_project_and_raises_ProjectCreatedEvent()
    {
        var project = Create();

        project.OrganisationId.Should().Be(OrgId);
        project.Name.Value.Should().Be("Website Revamp");
        project.Description.Should().Be("A description");
        project.Status.Should().Be(ProjectStatus.Active);
        project.CreatedById.Should().Be(CreatorId);
        project.IsArchived.Should().BeFalse();
        project.DeletedAt.Should().BeNull();

        var @event = project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectCreatedEvent>().Subject;
        @event.ProjectId.Should().Be(project.Id);
        @event.OrganisationId.Should().Be(OrgId);
        @event.CreatedById.Should().Be(CreatorId);
    }

    [Fact]
    public void Update_changes_the_details_when_active()
    {
        var project = Create();

        project.Update(ProjectName.Create("New Name"), "New description");

        project.Name.Value.Should().Be("New Name");
        project.Description.Should().Be("New description");
    }

    [Fact]
    public void Update_is_rejected_while_archived()
    {
        var project = Create();
        project.Archive(CreatorId);

        var act = () => project.Update(ProjectName.Create("New Name"), null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archive_sets_the_status_and_raises_ProjectArchivedEvent()
    {
        var project = Create();
        project.ClearDomainEvents();

        project.Archive(CreatorId);

        project.IsArchived.Should().BeTrue();
        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectArchivedEvent>();
    }

    [Fact]
    public void Archive_is_idempotent_and_raises_no_second_event()
    {
        var project = Create();
        project.Archive(CreatorId);
        project.ClearDomainEvents();

        project.Archive(CreatorId);

        project.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Restore_reactivates_an_archived_project_and_raises_ProjectRestoredEvent()
    {
        var project = Create();
        project.Archive(CreatorId);
        project.ClearDomainEvents();

        project.Restore(CreatorId);

        project.IsArchived.Should().BeFalse();
        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectRestoredEvent>();
    }

    [Fact]
    public void Restore_on_an_active_project_is_a_no_op()
    {
        var project = Create();
        project.ClearDomainEvents();

        project.Restore(CreatorId);

        project.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Delete_soft_deletes_and_raises_ProjectDeletedEvent()
    {
        var project = Create();
        project.ClearDomainEvents();

        project.Delete(CreatorId);

        project.DeletedAt.Should().NotBeNull();
        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectDeletedEvent>();
    }
}
