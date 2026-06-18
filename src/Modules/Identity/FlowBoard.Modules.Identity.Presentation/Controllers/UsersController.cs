using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Application.Commands.UpdateUserProfile;
using FlowBoard.Modules.Identity.Application.Queries.GetUserProfile;
using FlowBoard.Modules.Identity.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Identity.Presentation.Controllers;

/// <summary>
/// User profile endpoints under <c>/api/v1/users</c>. All require an authenticated caller; the
/// account acted on is always the caller's own (<c>me</c>).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>Returns the authenticated user's profile.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>200 OK</c> with the profile; <c>401 Unauthorized</c> if not authenticated.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserProfileQuery(), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "user-not-found");
    }

    /// <summary>Updates the authenticated user's profile.</summary>
    /// <param name="request">The new profile details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the updated profile; <c>401 Unauthorized</c> if not authenticated;
    /// <c>422 Unprocessable Entity</c> if the body fails validation.
    /// </returns>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateUserProfileCommand(request.DisplayName),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "user-not-found");
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
