using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application;
using FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskStatus;
using FlowBoard.Modules.Tasks.Application.Commands.CreateTask;
using FlowBoard.Modules.Tasks.Application.Commands.DeleteTask;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Tasks.UnitTests.Application;

public sealed class TaskCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ITenantContext> _tenant = new();
    private readonly Mock<IProjectReader> _projectReader = new();
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly OrganisationId _orgId = OrganisationId.New();
    private readonly UserId _userId = UserId.New();
    private readonly ProjectId _projectId = ProjectId.New();

    public TaskCommandHandlerTests()
    {
        _tenant.Setup(t => t.CurrentOrganisationId).Returns(_orgId);
        _currentUser.Setup(c => c.UserId).Returns(_userId);
    }

    private CreateTaskCommandHandler CreateHandler() =>
        new(_currentUser.Object, _tenant.Object, _projectReader.Object, _tasks.Object);

    private void ProjectIs(ProjectAvailability availability) =>
        _projectReader
            .Setup(r => r.GetAvailabilityAsync(It.IsAny<ProjectId>(), _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);

    private TaskItem Seed()
    {
        var task = TaskItem.Create(_projectId, _orgId, "Seed", null, Priority.Medium, _userId);
        _tasks
            .Setup(r => r.GetByIdAsync(It.IsAny<TaskId>(), _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        return task;
    }

    [Fact]
    public async Task Create_returns_project_not_found_when_the_project_is_missing()
    {
        ProjectIs(ProjectAvailability.NotFound);

        var result = await CreateHandler().Handle(
            new CreateTaskCommand(_projectId.Value, "Title", null, "Medium"), CancellationToken.None);

        result.Error.Should().Be(TaskErrors.ProjectNotFound);
    }

    [Fact]
    public async Task Create_returns_project_archived_when_the_project_is_archived()
    {
        ProjectIs(ProjectAvailability.Archived);

        var result = await CreateHandler().Handle(
            new CreateTaskCommand(_projectId.Value, "Title", null, "Medium"), CancellationToken.None);

        result.Error.Should().Be(TaskErrors.ProjectArchived);
    }

    [Fact]
    public async Task Create_creates_the_task_in_an_active_project()
    {
        ProjectIs(ProjectAvailability.Active);

        var result = await CreateHandler().Handle(
            new CreateTaskCommand(_projectId.Value, "Title", "desc", "High"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Todo");
        result.Value.Priority.Should().Be("High");
        _tasks.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_throws_on_an_invalid_transition()
    {
        Seed();  // Todo
        var handler = new ChangeTaskStatusCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object);

        var act = () => handler.Handle(
            new ChangeTaskStatusCommand(Guid.NewGuid(), "Done"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }

    [Fact]
    public async Task ChangeStatus_applies_a_valid_transition()
    {
        var task = Seed();  // Todo
        var handler = new ChangeTaskStatusCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object);

        var result = await handler.Handle(
            new ChangeTaskStatusCommand(Guid.NewGuid(), "InProgress"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public async Task Delete_returns_not_found_when_the_task_is_missing()
    {
        _tasks
            .Setup(r => r.GetByIdAsync(It.IsAny<TaskId>(), _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        var handler = new DeleteTaskCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object);

        var result = await handler.Handle(new DeleteTaskCommand(Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(TaskErrors.NotFound);
    }
}
