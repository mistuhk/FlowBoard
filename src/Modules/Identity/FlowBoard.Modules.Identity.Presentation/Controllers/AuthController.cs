using System.Security.Claims;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Commands.LoginUser;
using FlowBoard.Modules.Identity.Application.Commands.Logout;
using FlowBoard.Modules.Identity.Application.Commands.RefreshToken;
using FlowBoard.Modules.Identity.Application.Commands.RegisterUser;
using FlowBoard.Modules.Identity.Application.Commands.RequestPasswordReset;
using FlowBoard.Modules.Identity.Application.Commands.ResetPassword;
using FlowBoard.Modules.Identity.Application.Commands.VerifyEmail;
using FlowBoard.Modules.Identity.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Authenticates a user and issues an access token plus a refresh token.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with the access token on success, and the refresh token set as an httpOnly
    /// cookie; <c>401 Unauthorized</c> if the credentials are wrong; <c>403 Forbidden</c> if the
    /// account's email is not yet verified; <c>422 Unprocessable Entity</c> if the body fails
    /// validation.
    /// </returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginUserCommand(request.Email, request.Password),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error == IdentityErrors.EmailNotVerified
                ? ToProblem(result.Error, StatusCodes.Status403Forbidden, "Forbidden", "email-not-verified")
                : ToProblem(result.Error, StatusCodes.Status401Unauthorized, "Unauthorized", "invalid-credentials");
        }

        var response = result.Value;
        SetRefreshTokenCookie(response.RefreshToken);

        return Ok(new LoginResponse(response.AccessToken, response.AccessTokenExpiresAtUtc));
    }

    /// <summary>
    /// Exchanges the refresh token in the request cookie for a new access token, rotating the
    /// refresh token in the process.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>200 OK</c> with a new access token, and a rotated refresh token set as an httpOnly
    /// cookie; <c>401 Unauthorized</c> if the cookie is missing or the token is invalid, expired,
    /// or already used.
    /// </returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            return ToProblem(IdentityErrors.InvalidRefreshToken, StatusCodes.Status401Unauthorized, "Unauthorized", "invalid-refresh-token");

        var result = await sender.Send(new RefreshTokenCommand(refreshToken), cancellationToken);

        if (result.IsFailure)
        {
            ClearRefreshTokenCookie();
            return ToProblem(result.Error, StatusCodes.Status401Unauthorized, "Unauthorized", "invalid-refresh-token");
        }

        var response = result.Value;
        SetRefreshTokenCookie(response.RefreshToken);

        return Ok(new LoginResponse(response.AccessToken, response.AccessTokenExpiresAtUtc));
    }

    /// <summary>
    /// Logs the current session out: revokes the access token and deletes the refresh token. The
    /// caller must present a valid access token.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>204 No Content</c> once the session is revoked.</returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var tokenId = User.FindFirstValue("jti");
        var expiry = User.FindFirstValue("exp");
        if (string.IsNullOrEmpty(tokenId) || !long.TryParse(expiry, out var expiryUnixSeconds))
            return Unauthorized();

        var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expiryUnixSeconds).UtcDateTime;
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        await sender.Send(new LogoutCommand(tokenId, expiresAtUtc, refreshToken), cancellationToken);

        ClearRefreshTokenCookie();
        return NoContent();
    }

    /// <summary>
    /// Starts the password-reset flow for an email address. Always returns the same response so it
    /// cannot be used to discover whether an email is registered.
    /// </summary>
    /// <param name="request">The email address to send a reset link to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>202 Accepted</c> regardless of whether the email belongs to an account.</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new RequestPasswordResetCommand(request.Email), cancellationToken);
        return Accepted();
    }

    /// <summary>
    /// Completes a password reset: sets a new password from a valid reset token and revokes the
    /// account's existing sessions.
    /// </summary>
    /// <param name="request">The reset token and the new password.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// <c>204 No Content</c> when the password is changed; <c>400 Bad Request</c> if the token is
    /// invalid, expired, or already used; <c>422 Unprocessable Entity</c> if the new password fails
    /// the strength policy.
    /// </returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status400BadRequest, "Bad Request", "invalid-password-reset-token");
    }

    /// <summary>The name of the cookie carrying the refresh token.</summary>
    private const string RefreshTokenCookieName = "refresh_token";

    /// <summary>The path the refresh cookie is scoped to, matched on both set and delete.</summary>
    private const string RefreshTokenCookiePath = "/api/v1/auth";

    /// <summary>
    /// Writes the refresh token as an httpOnly, Secure, SameSite=Strict cookie scoped to the auth
    /// endpoints, so it is never readable by client script and is only returned to the refresh and
    /// logout routes.
    /// </summary>
    private void SetRefreshTokenCookie(string refreshToken) =>
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = RefreshTokenCookiePath,
            MaxAge = RefreshTokens.Ttl,
        });

    /// <summary>Removes the refresh cookie, using the same attributes it was set with.</summary>
    private void ClearRefreshTokenCookie() =>
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = RefreshTokenCookiePath,
        });

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
