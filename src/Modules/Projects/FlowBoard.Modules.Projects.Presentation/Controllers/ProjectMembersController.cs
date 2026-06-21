using FlowBoard.Application.Authorization;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Projects.Application;
using FlowBoard.Modules.Projects.Application.Commands.AddProjectMember;
using FlowBoard.Modules.Projects.Application.Commands.RemoveProjectMember;
using FlowBoard.Modules.Projects.Application.Queries.GetProjectMembers;
using FlowBoard.Modules.Projects.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Projects.Presentation.Controllers;

/// <summary>
/// Project membership endpoints under <c>/api/v1/organisations/{orgId}/projects/{projectId}/members</c>.
/// Listing requires organisation membership (and respects Guest visibility); adding and removing
/// members require Admin or Owner.
/// </summary>
[ApiController]
[Authorize(Policy = OrganisationPolicies.Member)]
[Route("api/v1/organisations/{orgId:guid}/projects/{projectId:guid}/members")]
public sealed class ProjectMembersController(ISender sender) : ControllerBase
{
    /// <summary>Lists the project's explicit members.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProjectMembersQuery(projectId), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "project-not-found");
    }

    /// <summary>Adds an organisation member to the project. Admin or Owner only.</summary>
    [HttpPost]
    [Authorize(Policy = OrganisationPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Add(
        Guid projectId,
        [FromBody] AddProjectMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddProjectMemberCommand(projectId, request.UserId), cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        return result.Error == ProjectErrors.NotFound
            ? ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "project-not-found")
            : ToProblem(result.Error, StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity", "project-member-invalid");
    }

    /// <summary>Removes a member from the project. Admin or Owner only.</summary>
    [HttpDelete("{userId:guid}")]
    [Authorize(Policy = OrganisationPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveProjectMemberCommand(projectId, userId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "project-not-found");
    }

    private ObjectResult ToProblem(Error error, int statusCode, string title, string type) =>
        new(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://flowboard.io/errors/{type}",
            Detail = error.Description,
            Extensions =
            {
                ["traceId"] = HttpContext.TraceIdentifier,
                ["code"] = error.Code,
            },
        })
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" },
        };
}
