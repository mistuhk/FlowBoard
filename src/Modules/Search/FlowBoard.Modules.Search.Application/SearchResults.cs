namespace FlowBoard.Modules.Search.Application;

/// <summary>
/// A task matched by a search, with its relevance rank. Highlighting of matched terms is performed
/// client-side from the query, so no server-generated snippet is returned.
/// </summary>
/// <param name="Id">The task id.</param>
/// <param name="ProjectId">The project the task belongs to.</param>
/// <param name="Title">The task title.</param>
/// <param name="Status">The task status token.</param>
/// <param name="Priority">The task priority token.</param>
/// <param name="Rank">The full-text relevance rank (0 when no text query was supplied).</param>
public sealed record TaskSearchResult(
    Guid Id, Guid ProjectId, string Title, string Status, string Priority, double Rank);

/// <summary>A project matched by a search, with its relevance rank.</summary>
/// <param name="Id">The project id.</param>
/// <param name="Name">The project name.</param>
/// <param name="Status">The project status token.</param>
/// <param name="Rank">The full-text relevance rank (0 when no text query was supplied).</param>
public sealed record ProjectSearchResult(Guid Id, string Name, string Status, double Rank);

/// <summary>Optional task filters applied alongside (or instead of) the text query.</summary>
/// <param name="Status">Filter by status token.</param>
/// <param name="Priority">Filter by priority token.</param>
/// <param name="AssigneeId">Filter by assignee.</param>
/// <param name="DueBefore">Only tasks due strictly before this instant.</param>
/// <param name="DueAfter">Only tasks due on or after this instant.</param>
public sealed record TaskSearchFilters(
    string? Status, string? Priority, Guid? AssigneeId, DateTime? DueBefore, DateTime? DueAfter);

/// <summary>The combined search response.</summary>
/// <param name="Tasks">Matching tasks, most relevant first.</param>
/// <param name="Projects">Matching projects, most relevant first.</param>
public sealed record SearchResults(IReadOnlyList<TaskSearchResult> Tasks, IReadOnlyList<ProjectSearchResult> Projects);
