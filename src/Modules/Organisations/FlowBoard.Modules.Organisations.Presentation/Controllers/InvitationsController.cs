using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Commands.AcceptInvitation;
using FlowBoard.Modules.Organisations.Application.Commands.DeclineInvitation;
using FlowBoard.Modules.Organisations.Application.Commands.InviteMember;
using FlowBoard.Modules.Organisations.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Organisations.Presentation.Controllers;

/// <summary>
/// Organisation invitation endpoints. Issuing an invitation is organisation-scoped and restricted
/// to Admins and the Owner (enforced by the aggregate). Accepting and declining are keyed by the
/// token alone, since the invitee is not yet a member.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1")]
public sealed class InvitationsController(ISender sender) : ControllerBase
{
    /// <summary>Invites a person to join the organisation. Admin or Owner only.</summary>
    /// <param name="orgId">The organisation to invite into.</param>
    /// <param name="request">The invitee's email and role.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>201 Created</c> with the invitation; <c>403 Forbidden</c> if the caller may not invite;
    /// <c>404 Not Found</c> if the organisation does not exist; <c>422</c> on a duplicate invite.
    /// </returns>
    [HttpPost("organisations/{orgId:guid}/invitations")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Invite(
        Guid orgId,
        [FromBody] InviteMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new InviteMemberCommand(orgId, request.Email, request.Role), cancellationToken);

        return result.IsSuccess
            ? Created(
                $"/api/v1/organisations/{orgId}/invitations/{result.Value.InvitationId}",
                result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "organisation-not-found");
    }

    /// <summary>Accepts an invitation by its token, joining the authenticated user.</summary>
    /// <param name="request">The invitation token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the joined organisation; <c>404 Not Found</c> for an unknown token;
    /// <c>422</c> if the invitation has expired, was already used, or the user is already a member.
    /// </returns>
    [HttpPost("invitations/accept")]
    [ProducesResponseType(typeof(OrganisationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Accept(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AcceptInvitationCommand(request.Token), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "invitation-not-found");
    }

    /// <summary>Declines an invitation by its token.</summary>
    /// <param name="request">The invitation token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>204 No Content</c> on success; <c>404 Not Found</c> for an unknown token.</returns>
    [HttpPost("invitations/decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Decline(
        [FromBody] DeclineInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeclineInvitationCommand(request.Token), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "invitation-not-found");
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
