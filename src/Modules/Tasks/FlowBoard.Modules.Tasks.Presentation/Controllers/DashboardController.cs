using FlowBoard.Modules.Tasks.Application.Dashboard;
using FlowBoard.Modules.Tasks.Application.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Tasks.Presentation.Controllers;

/// <summary>
/// The current user's dashboard at <c>GET /api/v1/dashboard</c>. Scoped to the authenticated user and
/// spans every organisation they belong to, so it is not organisation-scoped. Served from a
/// short-lived Redis cache.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/dashboard")]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    /// <summary>Returns the current user's dashboard snapshot.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var dashboard = await sender.Send(new GetDashboardQuery(), cancellationToken);
        return Ok(dashboard);
    }
}
