using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Commands.ChangeMemberRole;
using FlowBoard.Modules.Organisations.Application.Commands.RemoveMember;
using FlowBoard.Modules.Organisations.Application.Commands.TransferOwnership;
using FlowBoard.Modules.Organisations.Application.Queries.ListMembers;
using FlowBoard.Modules.Organisations.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Organisations.Presentation.Controllers;

/// <summary>
/// Organisation membership management endpoints under <c>/api/v1/organisations/{orgId}/members</c>,
/// plus ownership transfer. Authorisation (outranking, owner-only transfer) is enforced by the
/// aggregate; a disallowed action yields <c>403 Forbidden</c>, and an invariant violation
/// (for example removing the Owner) yields <c>422</c>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/organisations/{orgId:guid}")]
public sealed class MembersController(ISender sender) : ControllerBase
{
    /// <summary>Lists the organisation's members. The caller must be a member of the organisation.</summary>
    /// <param name="orgId">The organisation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    [HttpGet("members")]
    [ProducesResponseType(typeof(IReadOnlyList<MemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid orgId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListMembersQuery(orgId), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "organisation-not-found");
    }

    /// <summary>Changes a member's role.</summary>
    /// <param name="orgId">The organisation.</param>
    /// <param name="userId">The member whose role is changing.</param>
    /// <param name="request">The new role.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>204 No Content</c> on success; <c>403</c>/<c>404</c>/<c>422</c> on failure.</returns>
    [HttpPut("members/{userId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeRole(
        Guid orgId,
        Guid userId,
        [FromBody] ChangeMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeMemberRoleCommand(orgId, userId, request.Role), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "organisation-not-found");
    }

    /// <summary>Removes a member from the organisation.</summary>
    /// <param name="orgId">The organisation.</param>
    /// <param name="userId">The member to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>204 No Content</c> on success; <c>403</c>/<c>404</c>/<c>422</c> on failure.</returns>
    [HttpDelete("members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Remove(
        Guid orgId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveMemberCommand(orgId, userId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "organisation-not-found");
    }

    /// <summary>Transfers organisation ownership to another member. Owner only.</summary>
    /// <param name="orgId">The organisation.</param>
    /// <param name="request">The new owner.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>204 No Content</c> on success; <c>403</c>/<c>404</c>/<c>422</c> on failure.</returns>
    [HttpPut("owner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> TransferOwnership(
        Guid orgId,
        [FromBody] TransferOwnershipRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new TransferOwnershipCommand(orgId, request.NewOwnerId), cancellationToken);

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
