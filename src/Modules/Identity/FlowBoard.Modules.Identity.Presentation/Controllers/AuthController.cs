using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Application.Commands.RegisterUser;
using FlowBoard.Modules.Identity.Application.Commands.VerifyEmail;
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

    /// <summary>
    /// Confirms a user's email address from the single-use token in their verification link.
    /// </summary>
    /// <param name="token">The verification token issued at registration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>204 No Content</c> when the address is confirmed; <c>400 Bad Request</c> if the token
    /// is invalid, expired, or already used; <c>422 Unprocessable Entity</c> if no token is supplied.
    /// </returns>
    [HttpGet("verify-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifyEmailCommand(token), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status400BadRequest, "Bad Request", "invalid-verification-token");
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
