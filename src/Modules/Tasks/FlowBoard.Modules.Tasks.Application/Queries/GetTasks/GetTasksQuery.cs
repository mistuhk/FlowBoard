using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetTasks;

/// <summary>
/// Lists tasks in a project, newest first, with optional filters and opaque cursor pagination.
/// </summary>
/// <param name="ProjectId">The project whose tasks to list.</param>
/// <param name="Status">Optional status filter (Todo, InProgress, Blocked, Done).</param>
/// <param name="Priority">Optional priority filter (Low, Medium, High, Critical).</param>
/// <param name="AssigneeId">Optional assignee filter.</param>
/// <param name="DueBefore">Optional upper bound (exclusive) on the due date.</param>
/// <param name="DueAfter">Optional lower bound (exclusive) on the due date.</param>
/// <param name="Cursor">Opaque cursor from a previous page, or <c>null</c> for the first page.</param>
/// <param name="Limit">Page size (1 to 100; defaults to 20).</param>
public sealed record GetTasksQuery(
    Guid ProjectId,
    string? Status,
    string? Priority,
    Guid? AssigneeId,
    DateTime? DueBefore,
    DateTime? DueAfter,
    string? Cursor,
    int? Limit) : IQuery<TaskPageResponse>;
