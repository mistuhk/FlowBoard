using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Repositories;

/// <summary>
/// Filter and keyset-cursor criteria for a page of tasks. Ordering is newest first
/// (<c>CreatedAt</c> descending, then <c>Id</c>); <see cref="CursorCreatedAt"/> bounds the page to
/// tasks created before the last one returned.
/// </summary>
/// <param name="OrganisationId">The organisation to query within.</param>
/// <param name="ProjectId">The project whose tasks are listed.</param>
/// <param name="Status">Optional status filter.</param>
/// <param name="Priority">Optional priority filter.</param>
/// <param name="AssigneeId">Optional assignee filter.</param>
/// <param name="DueBefore">Optional upper bound (exclusive) on the due date.</param>
/// <param name="DueAfter">Optional lower bound (exclusive) on the due date.</param>
/// <param name="CursorCreatedAt">The created-at of the last task from the previous page, or <c>null</c> for the first page.</param>
/// <param name="Limit">The maximum number of tasks to return.</param>
public sealed record TaskQuery(
    OrganisationId OrganisationId,
    ProjectId ProjectId,
    TaskItemStatus? Status,
    Priority? Priority,
    UserId? AssigneeId,
    DateTime? DueBefore,
    DateTime? DueAfter,
    DateTime? CursorCreatedAt,
    int Limit);

/// <summary>
/// Persistence abstraction for the <see cref="TaskItem"/> aggregate. Every lookup is scoped to an
/// organisation, the primary tenant-isolation mechanism for reads.
/// </summary>
public interface ITaskRepository
{
    /// <summary>Loads a tracked task by id within an organisation, or <c>null</c> if not found.</summary>
    /// <param name="id">The task id.</param>
    /// <param name="organisationId">The organisation the caller is acting within.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<TaskItem?> GetByIdAsync(
        TaskId id,
        OrganisationId organisationId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a page of tasks matching the criteria, newest first.</summary>
    /// <param name="query">The filter and cursor criteria.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<TaskItem>> GetPageAsync(TaskQuery query, CancellationToken cancellationToken = default);

    /// <summary>Adds a newly created task to the unit of work.</summary>
    /// <param name="task">The task to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
}
