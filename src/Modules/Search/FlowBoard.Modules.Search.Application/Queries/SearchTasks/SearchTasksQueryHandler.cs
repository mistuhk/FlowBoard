using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Search.Application.Abstractions;
using MediatR;

namespace FlowBoard.Modules.Search.Application.Queries.SearchTasks;

/// <summary>Handles <see cref="SearchTasksQuery"/>: org-scoped full-text task search. Read-only.</summary>
public sealed class SearchTasksQueryHandler(
    ITenantContext tenantContext,
    ISearchRepository search)
    : IRequestHandler<SearchTasksQuery, IReadOnlyList<TaskSearchResult>>
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 50;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskSearchResult>> Handle(SearchTasksQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit ?? DefaultLimit, 1, MaxLimit);
        var filters = new TaskSearchFilters(
            request.Status, request.Priority, request.AssigneeId, request.DueBefore, request.DueAfter);

        return await search.SearchTasksAsync(
            tenantContext.CurrentOrganisationId.Value, request.Query, filters, limit, cancellationToken);
    }
}
