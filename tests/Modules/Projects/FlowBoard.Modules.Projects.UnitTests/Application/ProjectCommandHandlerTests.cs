using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Application;
using FlowBoard.Modules.Projects.Application.Commands.ArchiveProject;
using FlowBoard.Modules.Projects.Application.Commands.CreateProject;
using FlowBoard.Modules.Projects.Application.Commands.DeleteProject;
using FlowBoard.Modules.Projects.Application.Commands.UpdateProject;
using FlowBoard.Modules.Projects.Domain.Aggregates;
using FlowBoard.Modules.Projects.Domain.Repositories;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Projects.UnitTests.Application;

public sealed class ProjectCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ITenantContext> _tenant = new();
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly OrganisationId _orgId = OrganisationId.New();
    private readonly UserId _userId = UserId.New();

    public ProjectCommandHandlerTests()
    {
        _tenant.Setup(t => t.CurrentOrganisationId).Returns(_orgId);
        _currentUser.Setup(c => c.UserId).Returns(_userId);
    }

    private Project SeedProject(bool archived = false)
    {
        var project = Project.Create(_orgId, ProjectName.Create("Seed"), null, _userId);
        if (archived)
            project.Archive(_userId);
        _projects
            .Setup(r => r.GetByIdAsync(It.IsAny<ProjectId>(), _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        return project;
    }

    [Fact]
    public async Task Create_creates_the_project_in_the_current_tenant_with_the_caller_as_creator()
    {
        var handler = new CreateProjectCommandHandler(_currentUser.Object, _tenant.Object, _projects.Object);

        var result = await handler.Handle(new CreateProjectCommand("Website", "desc"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrganisationId.Should().Be(_orgId.Value);
        result.Value.CreatedById.Should().Be(_userId.Value);
        result.Value.Status.Should().Be("Active");
        _projects.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_returns_not_found_when_the_project_is_missing()
    {
        _projects
            .Setup(r => r.GetByIdAsync(It.IsAny<ProjectId>(), _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);
        var handler = new UpdateProjectCommandHandler(_tenant.Object, _projects.Object);

        var result = await handler.Handle(
            new UpdateProjectCommand(Guid.NewGuid(), "New", null), CancellationToken.None);

        result.Error.Should().Be(ProjectErrors.NotFound);
    }

    [Fact]
    public async Task Update_applies_the_change_when_active()
    {
        var project = SeedProject();
        var handler = new UpdateProjectCommandHandler(_tenant.Object, _projects.Object);

        var result = await handler.Handle(
            new UpdateProjectCommand(project.Id.Value, "Renamed", "new"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        project.Name.Value.Should().Be("Renamed");
    }

    [Fact]
    public async Task Update_throws_when_the_project_is_archived()
    {
        var project = SeedProject(archived: true);
        var handler = new UpdateProjectCommandHandler(_tenant.Object, _projects.Object);

        var act = () => handler.Handle(
            new UpdateProjectCommand(project.Id.Value, "Renamed", null), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Archive_returns_not_found_when_the_project_is_missing()
    {
        _projects
            .Setup(r => r.GetByIdAsync(It.IsAny<ProjectId>(), _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);
        var handler = new ArchiveProjectCommandHandler(_currentUser.Object, _tenant.Object, _projects.Object);

        var result = await handler.Handle(new ArchiveProjectCommand(Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(ProjectErrors.NotFound);
    }

    [Fact]
    public async Task Delete_soft_deletes_the_project()
    {
        var project = SeedProject();
        var handler = new DeleteProjectCommandHandler(_currentUser.Object, _tenant.Object, _projects.Object);

        var result = await handler.Handle(new DeleteProjectCommand(project.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        project.DeletedAt.Should().NotBeNull();
    }
}
