using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Tasks.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ITaskRepository"/>. Lookups are organisation-scoped.
/// </summary>
internal sealed class TaskRepository(AppDbContext context) : ITaskRepository
{
    /// <inheritdoc/>
    public Task<TaskItem?> GetByIdAsync(
        TaskId id,
        OrganisationId organisationId,
        CancellationToken cancellationToken = default) =>
        context.Set<TaskItem>()
            .FirstOrDefaultAsync(t => t.Id == id && t.OrganisationId == organisationId, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskItem>> GetPageAsync(
        TaskQuery query,
        CancellationToken cancellationToken = default)
    {
        var tasks = context.Set<TaskItem>()
            .AsNoTracking()
            .Where(t => t.OrganisationId == query.OrganisationId && t.ProjectId == query.ProjectId);

        if (query.Status is not null)
            tasks = tasks.Where(t => t.Status == query.Status);

        if (query.Priority is not null)
            tasks = tasks.Where(t => t.Priority == query.Priority);

        if (query.AssigneeId is not null)
            tasks = tasks.Where(t => t.AssigneeId == query.AssigneeId);

        if (query.DueBefore is not null)
            tasks = tasks.Where(t => t.DueDate != null && t.DueDate < query.DueBefore);

        if (query.DueAfter is not null)
            tasks = tasks.Where(t => t.DueDate != null && t.DueDate > query.DueAfter);

        // Keyset cursor: tasks created strictly before the last one returned on the previous page.
        if (query.CursorCreatedAt is not null)
            tasks = tasks.Where(t => t.CreatedAt < query.CursorCreatedAt);

        return await tasks
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default) =>
        await context.Set<TaskItem>().AddAsync(task, cancellationToken);
}
