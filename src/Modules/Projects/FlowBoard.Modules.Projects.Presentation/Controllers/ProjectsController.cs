using FlowBoard.Application.Authorization;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Projects.Application;
using FlowBoard.Modules.Projects.Application.Commands.ArchiveProject;
using FlowBoard.Modules.Projects.Application.Commands.CreateProject;
using FlowBoard.Modules.Projects.Application.Commands.DeleteProject;
using FlowBoard.Modules.Projects.Application.Commands.RestoreProject;
using FlowBoard.Modules.Projects.Application.Commands.UpdateProject;
using FlowBoard.Modules.Projects.Application.Queries.GetProject;
using FlowBoard.Modules.Projects.Application.Queries.ListProjects;
using FlowBoard.Modules.Projects.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Projects.Presentation.Controllers;

/// <summary>
/// Project endpoints under <c>/api/v1/organisations/{orgId}/projects</c>. Reads require organisation
/// membership; mutations require Admin or Owner. The organisation is resolved from the route by the
/// tenant middleware, and access is enforced by the organisation authorisation policies.
/// </summary>
[ApiController]
[Authorize(Policy = OrganisationPolicies.Member)]
[Route("api/v1/organisations/{orgId:guid}/projects")]
public sealed class ProjectsController(ISender sender) : ControllerBase
{
    /// <summary>Lists the active projects in the organisation.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ListProjectsQuery(), cancellationToken));

    /// <summary>Returns a single project.</summary>
    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProjectQuery(projectId), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "project-not-found");
    }

    /// <summary>Creates a project. Admin or Owner only.</summary>
    [HttpPost]
    [Authorize(Policy = OrganisationPolicies.Admin)]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateProjectCommand(request.Name, request.Description), cancellationToken);

        return Created(
            $"/api/v1/organisations/{result.Value.OrganisationId}/projects/{result.Value.Id}",
            result.Value);
    }

    /// <summary>Updates a project's details. Admin or Owner only. Rejected with 422 while archived.</summary>
    [HttpPut("{projectId:guid}")]
    [Authorize(Policy = OrganisationPolicies.Admin)]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        Guid projectId,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateProjectCommand(projectId, request.Name, request.Description), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "project-not-found");
    }

    /// <summary>Archives a project. Admin or Owner only.</summary>
    [HttpPut("{projectId:guid}/archive")]
    [Authorize(Policy = OrganisationPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ArchiveProjectCommand(projectId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "project-not-found");
    }

    /// <summary>Restores an archived project. Admin or Owner only.</summary>
    [HttpPut("{projectId:guid}/restore")]
    [Authorize(Policy = OrganisationPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RestoreProjectCommand(projectId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "project-not-found");
    }

    /// <summary>Soft-deletes a project. Admin or Owner only.</summary>
    [HttpDelete("{projectId:guid}")]
    [Authorize(Policy = OrganisationPolicies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProjectCommand(projectId), cancellationToken);
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
