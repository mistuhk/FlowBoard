namespace FlowBoard.Modules.Search.Application.Abstractions;

/// <summary>
/// Read-only full-text search over tenant-scoped data. Implemented in the Search Infrastructure layer
/// with raw PostgreSQL FTS (<c>websearch_to_tsquery</c>, <c>ts_rank</c>, <c>ts_headline</c>). Every
/// query is scoped to the organisation.
/// </summary>
public interface ISearchRepository
{
    /// <summary>
    /// Searches tasks. When <paramref name="query"/> is null/blank, the text predicate and ranking are
    /// skipped and only the filters apply (ordered newest first); otherwise results are ranked by relevance.
    /// </summary>
    Task<IReadOnlyList<TaskSearchResult>> SearchTasksAsync(
        Guid organisationId, string? query, TaskSearchFilters filters, int limit, CancellationToken cancellationToken = default);

    /// <summary>Searches projects by name and description. Ranked by relevance when a query is supplied.</summary>
    Task<IReadOnlyList<ProjectSearchResult>> SearchProjectsAsync(
        Guid organisationId, string? query, int limit, CancellationToken cancellationToken = default);
}
