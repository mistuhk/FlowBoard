using FlowBoard.Application.Authorization;
using FlowBoard.Modules.ActivityLog.Application;
using FlowBoard.Modules.ActivityLog.Application.Queries.GetProjectActivity;
using FlowBoard.Modules.ActivityLog.Application.Queries.GetUserActivity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.ActivityLog.Presentation.Controllers;

/// <summary>
/// Activity feeds under <c>/api/v1/organisations/{orgId}</c>. Requires organisation membership; reads
/// are scoped to the organisation resolved from the route.
/// </summary>
[ApiController]
[Authorize(Policy = OrganisationPolicies.Member)]
[Route("api/v1/organisations/{orgId:guid}")]
public sealed class ActivityController(ISender sender) : ControllerBase
{
    /// <summary>Lists project-level activity, newest first.</summary>
    [HttpGet("projects/{projectId:guid}/activity")]
    [ProducesResponseType(typeof(ActivityPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProjectActivity(Guid projectId, [FromQuery] string? cursor, [FromQuery] int? limit, CancellationToken cancellationToken)
    {
        var page = await sender.Send(new GetProjectActivityQuery(projectId, cursor, limit), cancellationToken);
        return Ok(page);
    }

    /// <summary>Lists the current user's activity within the organisation, newest first.</summary>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(ActivityPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MyActivity([FromQuery] string? cursor, [FromQuery] int? limit, CancellationToken cancellationToken)
    {
        var page = await sender.Send(new GetUserActivityQuery(cursor, limit), cancellationToken);
        return Ok(page);
    }
}
