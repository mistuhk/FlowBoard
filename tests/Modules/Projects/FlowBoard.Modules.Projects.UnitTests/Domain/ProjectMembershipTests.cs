using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Aggregates;
using FlowBoard.Modules.Projects.Domain.Events;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Projects.UnitTests.Domain;

public sealed class ProjectMembershipTests
{
    private static Project Create() =>
        Project.Create(OrganisationId.New(), ProjectName.Create("Website"), null, UserId.New());

    [Fact]
    public void AddMember_adds_the_member_and_raises_ProjectMemberAddedEvent()
    {
        var project = Create();
        project.ClearDomainEvents();
        var userId = UserId.New();

        project.AddMember(userId);

        project.HasMember(userId).Should().BeTrue();
        project.Members.Should().ContainSingle(m => m.UserId == userId);
        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectMemberAddedEvent>()
            .Which.UserId.Should().Be(userId);
    }

    [Fact]
    public void AddMember_is_idempotent_and_raises_no_second_event()
    {
        var project = Create();
        var userId = UserId.New();
        project.AddMember(userId);
        project.ClearDomainEvents();

        project.AddMember(userId);

        project.Members.Count(m => m.UserId == userId).Should().Be(1);
        project.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMember_removes_an_existing_member()
    {
        var project = Create();
        var userId = UserId.New();
        project.AddMember(userId);

        project.RemoveMember(userId);

        project.HasMember(userId).Should().BeFalse();
    }

    [Fact]
    public void RemoveMember_is_a_no_op_for_a_non_member()
    {
        var project = Create();

        var act = () => project.RemoveMember(UserId.New());

        act.Should().NotThrow();
    }
}
