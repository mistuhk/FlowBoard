using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application;
using FlowBoard.Modules.Tasks.Application.Commands.AssignTask;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Tasks.UnitTests.Application;

public sealed class AssignTaskCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ITenantContext> _tenant = new();
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IOrganisationMembershipReader> _membership = new();
    private readonly OrganisationId _orgId = OrganisationId.New();

    public AssignTaskCommandHandlerTests()
    {
        _tenant.Setup(t => t.CurrentOrganisationId).Returns(_orgId);
        _currentUser.Setup(c => c.UserId).Returns(UserId.New());
        var task = TaskItem.Create(ProjectId.New(), _orgId, "Seed", null, Priority.Medium, UserId.New());
        _tasks
            .Setup(r => r.GetByIdAsync(It.IsAny<TaskId>(), _orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
    }

    private AssignTaskCommandHandler CreateHandler() =>
        new(_currentUser.Object, _tenant.Object, _tasks.Object, _membership.Object);

    [Fact]
    public async Task Assign_rejects_an_assignee_who_is_not_an_organisation_member()
    {
        _membership
            .Setup(r => r.GetRoleNameAsync(_orgId, It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await CreateHandler().Handle(
            new AssignTaskCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(TaskErrors.AssigneeNotOrganisationMember);
    }

    [Fact]
    public async Task Assign_succeeds_for_an_organisation_member()
    {
        _membership
            .Setup(r => r.GetRoleNameAsync(_orgId, It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("member");

        var result = await CreateHandler().Handle(
            new AssignTaskCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
