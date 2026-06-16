using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Application.Commands.RegisterUser;
using FlowBoard.Modules.Identity.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Identity.Presentation.Controllers;

/// <summary>
/// Authentication endpoints under <c>/api/v1/auth</c>.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Registers a new user account and issues an email-verification token.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>201 Created</c> with the new user on success; <c>409 Conflict</c> if the email is
    /// already in use; <c>422 Unprocessable Entity</c> if the body fails validation.
    /// </returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterUserCommand(request.Email, request.Password, request.DisplayName),
            cancellationToken);

        if (result.IsFailure)
            return ToProblem(result.Error, StatusCodes.Status409Conflict, "Conflict", "conflict");

        var response = result.Value;
        return Created($"/api/v1/users/{response.Id}", response);
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
