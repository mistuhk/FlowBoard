using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application;
using FlowBoard.Modules.Tasks.Application.Queries.GetTasks;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Tasks.UnitTests.Application;

public sealed class TaskCursorAndPagingTests
{
    [Fact]
    public void Cursor_round_trips_a_timestamp()
    {
        var when = new DateTime(2026, 6, 21, 10, 30, 0, DateTimeKind.Utc).AddTicks(12345);

        var decoded = TaskCursor.Decode(TaskCursor.Encode(when));

        decoded.Should().Be(when);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-valid-cursor")]
    public void Decode_returns_null_for_missing_or_malformed_cursors(string? cursor)
    {
        TaskCursor.Decode(cursor).Should().BeNull();
    }

    [Fact]
    public async Task GetTasks_returns_a_next_cursor_when_more_pages_remain()
    {
        var orgId = OrganisationId.New();
        var projectId = ProjectId.New();
        var tenant = new Mock<ITenantContext>();
        tenant.Setup(t => t.CurrentOrganisationId).Returns(orgId);

        // Page size 2 -> the handler requests 3; return 3 so a further page is detected.
        var page = Enumerable.Range(0, 3)
            .Select(i => TaskItem.Create(projectId, orgId, $"Task {i}", null, Priority.Medium, UserId.New()))
            .ToList();
        var repo = new Mock<ITaskRepository>();
        repo.Setup(r => r.GetPageAsync(It.IsAny<TaskQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var handler = new GetTasksQueryHandler(tenant.Object, repo.Object);

        var result = await handler.Handle(
            new GetTasksQuery(projectId.Value, null, null, null, null, null, null, 2), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.NextCursor.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTasks_returns_no_cursor_on_the_last_page()
    {
        var orgId = OrganisationId.New();
        var projectId = ProjectId.New();
        var tenant = new Mock<ITenantContext>();
        tenant.Setup(t => t.CurrentOrganisationId).Returns(orgId);

        var page = new List<TaskItem>
        {
            TaskItem.Create(projectId, orgId, "Only", null, Priority.Low, UserId.New()),
        };
        var repo = new Mock<ITaskRepository>();
        repo.Setup(r => r.GetPageAsync(It.IsAny<TaskQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var handler = new GetTasksQueryHandler(tenant.Object, repo.Object);

        var result = await handler.Handle(
            new GetTasksQuery(projectId.Value, null, null, null, null, null, null, 20), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.NextCursor.Should().BeNull();
    }
}
