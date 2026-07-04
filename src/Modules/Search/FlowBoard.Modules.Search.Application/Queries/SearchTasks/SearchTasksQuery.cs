using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.Search.Application.Queries.SearchTasks;

/// <summary>Full-text search over tasks in the current organisation, with optional filters.</summary>
/// <param name="Query">The free-text query, or null/blank to filter only.</param>
/// <param name="Status">Optional status filter.</param>
/// <param name="Priority">Optional priority filter.</param>
/// <param name="AssigneeId">Optional assignee filter.</param>
/// <param name="DueBefore">Optional upper bound on due date (exclusive).</param>
/// <param name="DueAfter">Optional lower bound on due date (inclusive).</param>
/// <param name="Limit">Maximum results (clamped server-side).</param>
public sealed record SearchTasksQuery(
    string? Query,
    string? Status,
    string? Priority,
    Guid? AssigneeId,
    DateTime? DueBefore,
    DateTime? DueAfter,
    int? Limit) : IQuery<IReadOnlyList<TaskSearchResult>>;
