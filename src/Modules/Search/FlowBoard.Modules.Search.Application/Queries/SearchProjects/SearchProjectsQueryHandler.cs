using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Search.Application.Abstractions;
using MediatR;

namespace FlowBoard.Modules.Search.Application.Queries.SearchProjects;

/// <summary>Handles <see cref="SearchProjectsQuery"/>: org-scoped full-text project search. Read-only.</summary>
public sealed class SearchProjectsQueryHandler(
    ITenantContext tenantContext,
    ISearchRepository search)
    : IRequestHandler<SearchProjectsQuery, IReadOnlyList<ProjectSearchResult>>
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 50;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProjectSearchResult>> Handle(SearchProjectsQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit ?? DefaultLimit, 1, MaxLimit);
        return await search.SearchProjectsAsync(
            tenantContext.CurrentOrganisationId.Value, request.Query, limit, cancellationToken);
    }
}
