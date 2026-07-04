using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Search.Application;
using FlowBoard.Modules.Search.Application.Abstractions;
using FlowBoard.Modules.Search.Application.Queries.SearchProjects;
using FlowBoard.Modules.Search.Application.Queries.SearchTasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowBoard.Modules.Search.UnitTests;

public sealed class SearchHandlerTests
{
    private readonly Mock<ITenantContext> _tenant = new();
    private readonly Mock<ISearchRepository> _repo = new();
    private readonly OrganisationId _orgId = OrganisationId.New();

    public SearchHandlerTests() => _tenant.Setup(t => t.CurrentOrganisationId).Returns(_orgId);

    [Fact]
    public async Task SearchTasks_passes_org_query_and_filters_and_clamps_the_limit()
    {
        _repo.Setup(r => r.SearchTasksAsync(_orgId.Value, "login", It.IsAny<TaskSearchFilters>(), 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TaskSearchResult(Guid.NewGuid(), Guid.NewGuid(), "Fix login", "todo", "high", 0.5)]);
        var handler = new SearchTasksQueryHandler(_tenant.Object, _repo.Object);

        var results = await handler.Handle(
            new SearchTasksQuery("login", "Todo", null, null, null, null, 999), CancellationToken.None);

        results.Should().ContainSingle();
        _repo.Verify(r => r.SearchTasksAsync(
            _orgId.Value, "login",
            It.Is<TaskSearchFilters>(f => f.Status == "Todo"),
            50,  // clamped from 999
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchProjects_defaults_the_limit_and_scopes_to_the_org()
    {
        _repo.Setup(r => r.SearchProjectsAsync(_orgId.Value, "website", 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ProjectSearchResult(Guid.NewGuid(), "Marketing Website", "active", 0.3)]);
        var handler = new SearchProjectsQueryHandler(_tenant.Object, _repo.Object);

        var results = await handler.Handle(new SearchProjectsQuery("website", null), CancellationToken.None);

        results.Should().ContainSingle();
        _repo.Verify(r => r.SearchProjectsAsync(_orgId.Value, "website", 20, It.IsAny<CancellationToken>()), Times.Once);
    }
}
