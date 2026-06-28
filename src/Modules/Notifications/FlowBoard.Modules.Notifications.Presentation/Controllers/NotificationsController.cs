using FlowBoard.Modules.Notifications.Application;
using FlowBoard.Modules.Notifications.Application.Commands.MarkAllAsRead;
using FlowBoard.Modules.Notifications.Application.Commands.MarkAsRead;
using FlowBoard.Modules.Notifications.Application.Queries.GetNotifications;
using FlowBoard.Modules.Notifications.Application.Queries.GetUnreadCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Notifications.Presentation.Controllers;

/// <summary>
/// The current user's notification inbox under <c>/api/v1/notifications</c>. Every action is scoped to
/// the authenticated user, so one user can never see or modify another's notifications.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    /// <summary>The unread-count response.</summary>
    /// <param name="Count">The number of unread notifications.</param>
    public sealed record UnreadCountResponse(int Count);

    /// <summary>Lists the current user's notifications, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotificationPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? cursor, [FromQuery] int? limit, CancellationToken cancellationToken)
    {
        var page = await sender.Send(new GetNotificationsQuery(cursor, limit), cancellationToken);
        return Ok(page);
    }

    /// <summary>Returns the current user's unread-notification count.</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        var count = await sender.Send(new GetUnreadCountQuery(), cancellationToken);
        return Ok(new UnreadCountResponse(count));
    }

    /// <summary>Marks a single notification as read.</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkAsReadCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : ToProblem(result.Error);
    }

    /// <summary>Marks all of the current user's notifications as read.</summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await sender.Send(new MarkAllAsReadCommand(), cancellationToken);
        return NoContent();
    }

    private ObjectResult ToProblem(FlowBoard.Domain.Primitives.Error error) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Type = "https://flowboard.io/errors/notification-not-found",
            Detail = error.Description,
            Extensions =
            {
                ["traceId"] = HttpContext.TraceIdentifier,
                ["code"] = error.Code,
            },
        })
        {
            StatusCode = StatusCodes.Status404NotFound,
            ContentTypes = { "application/problem+json" },
        };
}
