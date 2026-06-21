using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Application;
using FlowBoard.Modules.Projects.Application.Commands.AddProjectMember;
using FlowBoard.Modules.Projects.Application.Queries.GetProject;
using FlowBoard.Modules.Projects.Application.Queries.ListProjects;
using FlowBoard.Modules.Projects.Domain.Aggregates;
using FlowBoard.Modules.Projects.Domain.Repositories;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Projects.UnitTests.Application;

public sealed class ProjectMembershipHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ITenantContext> _tenant = new();
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly Mock<IOrganisationMembershipReader> _membership = new();
    private readonly OrganisationId _orgId = OrganisationId.New();
    private readonly UserId _userId = UserId.New();

    public ProjectMembershipHandlerTests()
    {
        _tenant.Setup(t => t.CurrentOrganisationId).Returns(_orgId);
        _currentUser.Setup(c => c.UserId).Returns(_userId);
    }

    private Project Seed()
    {
        var project = Project.Create(_orgId, ProjectName.Create("Seed"), null, UserId.New());
        _projects
            .Setup(r => r.GetByIdAsync(It.IsAny<ProjectId>(), _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        return project;
    }

    private void CallerRole(string? roleName) =>
        _membership
            .Setup(r => r.GetRoleNameAsync(_orgId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleName);

    // -- AddProjectMember --

    [Fact]
    public async Task Add_rejects_a_user_who_is_not_an_organisation_member()
    {
        var project = Seed();
        var target = UserId.New();
        _membership
            .Setup(r => r.GetRoleNameAsync(_orgId, target, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var handler = new AddProjectMemberCommandHandler(_tenant.Object, _projects.Object, _membership.Object);

        var result = await handler.Handle(
            new AddProjectMemberCommand(project.Id.Value, target.Value), CancellationToken.None);

        result.Error.Should().Be(ProjectErrors.UserNotOrganisationMember);
        project.HasMember(target).Should().BeFalse();
    }

    [Fact]
    public async Task Add_adds_an_organisation_member_to_the_project()
    {
        var project = Seed();
        var target = UserId.New();
        _membership
            .Setup(r => r.GetRoleNameAsync(_orgId, target, It.IsAny<CancellationToken>()))
            .ReturnsAsync("member");
        var handler = new AddProjectMemberCommandHandler(_tenant.Object, _projects.Object, _membership.Object);

        var result = await handler.Handle(
            new AddProjectMemberCommand(project.Id.Value, target.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        project.HasMember(target).Should().BeTrue();
    }

    // -- Guest visibility on GetProject --

    [Fact]
    public async Task Get_hides_the_project_from_a_guest_who_is_not_a_member()
    {
        var project = Seed();
        CallerRole("guest");
        var handler = new GetProjectQueryHandler(_currentUser.Object, _tenant.Object, _projects.Object, _membership.Object);

        var result = await handler.Handle(new GetProjectQuery(project.Id.Value), CancellationToken.None);

        result.Error.Should().Be(ProjectErrors.NotFound);
    }

    [Fact]
    public async Task Get_returns_the_project_to_a_guest_who_is_a_member()
    {
        var project = Seed();
        project.AddMember(_userId);
        CallerRole("guest");
        var handler = new GetProjectQueryHandler(_currentUser.Object, _tenant.Object, _projects.Object, _membership.Object);

        var result = await handler.Handle(new GetProjectQuery(project.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Get_returns_the_project_to_a_non_guest_member_regardless_of_project_membership()
    {
        var project = Seed();
        CallerRole("member");
        var handler = new GetProjectQueryHandler(_currentUser.Object, _tenant.Object, _projects.Object, _membership.Object);

        var result = await handler.Handle(new GetProjectQuery(project.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // -- Guest visibility on ListProjects --

    [Fact]
    public async Task List_returns_only_the_projects_a_guest_belongs_to()
    {
        var visible = Project.Create(_orgId, ProjectName.Create("Visible"), null, UserId.New());
        visible.AddMember(_userId);
        var hidden = Project.Create(_orgId, ProjectName.Create("Hidden"), null, UserId.New());
        _projects
            .Setup(r => r.ListByOrganisationAsync(_orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([visible, hidden]);
        CallerRole("guest");
        var handler = new ListProjectsQueryHandler(_currentUser.Object, _tenant.Object, _projects.Object, _membership.Object);

        var result = await handler.Handle(new ListProjectsQuery(), CancellationToken.None);

        result.Should().ContainSingle(p => p.Id == visible.Id.Value);
    }
}
