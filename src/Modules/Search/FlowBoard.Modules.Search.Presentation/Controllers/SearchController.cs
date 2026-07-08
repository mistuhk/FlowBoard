using FlowBoard.Application.Authorization;
using FlowBoard.Modules.Search.Application;
using FlowBoard.Modules.Search.Application.Queries.SearchProjects;
using FlowBoard.Modules.Search.Application.Queries.SearchTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Search.Presentation.Controllers;

/// <summary>
/// Full-text search across the current organisation, at
/// <c>GET /api/v1/organisations/{orgId}/search</c>. Returns ranked tasks and projects. Requires
/// organisation membership; results are scoped to the organisation.
/// </summary>
[ApiController]
[Authorize(Policy = OrganisationPolicies.Member)]
[Route("api/v1/organisations/{orgId:guid}/search")]
public sealed class SearchController(ISender sender) : ControllerBase
{
    /// <summary>Searches tasks and/or projects. Filters apply to tasks.</summary>
    /// <param name="q">The free-text query (optional; with filters only, results are unranked).</param>
    /// <param name="type">What to search: <c>all</c> (default), <c>tasks</c>, or <c>projects</c>.</param>
    /// <param name="status">Optional task status filter.</param>
    /// <param name="priority">Optional task priority filter.</param>
    /// <param name="assigneeId">Optional task assignee filter.</param>
    /// <param name="dueBefore">Optional task due-before filter.</param>
    /// <param name="dueAfter">Optional task due-after filter.</param>
    /// <param name="limit">Maximum results per type.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResults), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] Guid? assigneeId,
        [FromQuery] DateTime? dueBefore,
        [FromQuery] DateTime? dueAfter,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var searchTasks = type is null || type.Equals("all", StringComparison.OrdinalIgnoreCase) || type.Equals("tasks", StringComparison.OrdinalIgnoreCase);
        var searchProjects = type is null || type.Equals("all", StringComparison.OrdinalIgnoreCase) || type.Equals("projects", StringComparison.OrdinalIgnoreCase);

        var tasks = searchTasks
            ? await sender.Send(new SearchTasksQuery(q, status, priority, assigneeId, dueBefore, dueAfter, limit), cancellationToken)
            : [];

        var projects = searchProjects
            ? await sender.Send(new SearchProjectsQuery(q, limit), cancellationToken)
            : [];

        return Ok(new SearchResults(tasks, projects));
    }
}
