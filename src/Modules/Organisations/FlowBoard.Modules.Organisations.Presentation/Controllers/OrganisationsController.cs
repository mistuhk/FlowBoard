using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Commands.CreateOrganisation;
using FlowBoard.Modules.Organisations.Application.Commands.DeleteOrganisation;
using FlowBoard.Modules.Organisations.Application.Commands.RenameOrganisation;
using FlowBoard.Modules.Organisations.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Organisations.Presentation.Controllers;

/// <summary>
/// Organisation lifecycle endpoints under <c>/api/v1/organisations</c>. All require an
/// authenticated caller. Renaming and deleting are restricted to the organisation's owner,
/// enforced by the aggregate (a non-owner attempt yields <c>403 Forbidden</c>).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/organisations")]
public sealed class OrganisationsController(ISender sender) : ControllerBase
{
    /// <summary>Creates a new organisation owned by the authenticated caller.</summary>
    /// <param name="request">The organisation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>201 Created</c> with the organisation; <c>409 Conflict</c> if the slug is taken.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrganisationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrganisationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateOrganisationCommand(request.Name, request.Slug), cancellationToken);

        return result.IsSuccess
            ? Created($"/api/v1/organisations/{result.Value.Id}", result.Value)
            : ToProblem(result.Error, StatusCodes.Status409Conflict, "Conflict", "organisation-slug-conflict");
    }

    /// <summary>Renames an organisation. Owner only.</summary>
    /// <param name="id">The organisation's identifier.</param>
    /// <param name="request">The new name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the updated organisation; <c>403 Forbidden</c> if the caller is not the
    /// owner; <c>404 Not Found</c> if no such organisation exists.
    /// </returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrganisationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Rename(
        Guid id,
        [FromBody] RenameOrganisationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RenameOrganisationCommand(id, request.Name), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "organisation-not-found");
    }

    /// <summary>Soft-deletes an organisation. Owner only.</summary>
    /// <param name="id">The organisation's identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>204 No Content</c> on success; <c>403 Forbidden</c> if the caller is not the owner;
    /// <c>404 Not Found</c> if no such organisation exists.
    /// </returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOrganisationCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "organisation-not-found");
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
