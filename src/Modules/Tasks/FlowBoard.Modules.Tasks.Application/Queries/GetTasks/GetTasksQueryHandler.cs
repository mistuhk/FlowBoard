using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetTasks;

/// <summary>
/// Handles <see cref="GetTasksQuery"/>: builds the filter and cursor criteria, fetches one extra row
/// to detect a further page, and returns the page with an opaque next cursor. Read-only.
/// </summary>
public sealed class GetTasksQueryHandler(
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<GetTasksQuery, TaskPageResponse>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    /// <inheritdoc/>
    public async Task<TaskPageResponse> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.Limit ?? DefaultPageSize, 1, MaxPageSize);

        var criteria = new TaskQuery(
            tenantContext.CurrentOrganisationId,
            ProjectId.From(request.ProjectId),
            string.IsNullOrWhiteSpace(request.Status) ? null : TaskItemStatus.FromName(request.Status),
            string.IsNullOrWhiteSpace(request.Priority) ? null : Priority.FromPersistence(request.Priority),
            request.AssigneeId.HasValue ? UserId.From(request.AssigneeId.Value) : null,
            request.DueBefore,
            request.DueAfter,
            TaskCursor.Decode(request.Cursor),
            pageSize + 1);  // one extra row tells us whether a further page exists

        var found = await tasks.GetPageAsync(criteria, cancellationToken);

        var hasMore = found.Count > pageSize;
        var page = hasMore ? found.Take(pageSize).ToList() : found.ToList();
        var nextCursor = hasMore ? TaskCursor.Encode(page[^1].CreatedAt) : null;

        return new TaskPageResponse(page.Select(TaskResponse.From).ToList(), nextCursor);
    }
}
