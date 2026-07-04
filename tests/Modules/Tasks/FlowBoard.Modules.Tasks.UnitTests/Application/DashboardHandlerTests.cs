using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application.Abstractions;
using FlowBoard.Modules.Tasks.Application.Dashboard;
using FlowBoard.Modules.Tasks.Application.EventHandlers;
using FlowBoard.Modules.Tasks.Application.Queries.GetDashboard;
using FlowBoard.Modules.Tasks.Domain.Events;
using FluentAssertions;
using MediatR;
using Moq;

namespace FlowBoard.Modules.Tasks.UnitTests.Application;

public sealed class DashboardHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IDashboardReader> _reader = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly UserId _userId = UserId.New();

    public DashboardHandlerTests() => _currentUser.Setup(c => c.UserId).Returns(_userId);

    private static DashboardResponse Sample() => new([], 0, [], [], 0d, []);

    [Fact]
    public async Task Returns_the_cached_dashboard_without_hitting_the_reader()
    {
        _cache.Setup(c => c.GetAsync<DashboardResponse>($"{_userId.Value}:dashboard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Sample());
        var handler = new GetDashboardQueryHandler(_currentUser.Object, _reader.Object, _cache.Object);

        await handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        _reader.Verify(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Builds_and_caches_on_a_miss()
    {
        _cache.Setup(c => c.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DashboardResponse?)null);
        _reader.Setup(r => r.GetAsync(_userId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(Sample());
        var handler = new GetDashboardQueryHandler(_currentUser.Object, _reader.Object, _cache.Object);

        await handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        _reader.Verify(r => r.GetAsync(_userId.Value, It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.SetAsync(
            $"{_userId.Value}:dashboard", It.IsAny<DashboardResponse>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Assignment_invalidates_the_assignee_dashboard()
    {
        var assignee = UserId.New();
        var handler = new TaskAssignedDashboardInvalidator(_cache.Object);
        var @event = new TaskAssignedEvent(TaskId.New(), OrganisationId.New(), assignee, UserId.New());

        await handler.Handle(new DomainEventNotification<TaskAssignedEvent>(@event), CancellationToken.None);

        _cache.Verify(c => c.RemoveAsync($"{assignee.Value}:dashboard", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unassignment_invalidates_the_previous_assignee_dashboard()
    {
        var previous = UserId.New();
        var handler = new TaskUnassignedDashboardInvalidator(_cache.Object);
        var @event = new TaskUnassignedEvent(TaskId.New(), OrganisationId.New(), previous);

        await handler.Handle(new DomainEventNotification<TaskUnassignedEvent>(@event), CancellationToken.None);

        _cache.Verify(c => c.RemoveAsync($"{previous.Value}:dashboard", It.IsAny<CancellationToken>()), Times.Once);
    }
}
