using FlowBoard.Application.Authorization;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Tasks.Application;
using FlowBoard.Modules.Tasks.Application.Commands.AddComment;
using FlowBoard.Modules.Tasks.Application.Commands.DeleteComment;
using FlowBoard.Modules.Tasks.Application.Commands.EditComment;
using FlowBoard.Modules.Tasks.Application.Queries.GetComments;
using FlowBoard.Modules.Tasks.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Tasks.Presentation.Controllers;

/// <summary>
/// Task comment endpoints under
/// <c>/api/v1/organisations/{orgId}/projects/{projectId}/tasks/{taskId}/comments</c>. Requires
/// organisation membership; editing and deleting are restricted (author or Admin/Owner) by the
/// handlers, which return 403 otherwise.
/// </summary>
[ApiController]
[Authorize(Policy = OrganisationPolicies.Member)]
[Route("api/v1/organisations/{orgId:guid}/projects/{projectId:guid}/tasks/{taskId:guid}/comments")]
public sealed class CommentsController(ISender sender) : ControllerBase
{
    /// <summary>Lists the task's active comments.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCommentsQuery(taskId), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");
    }

    /// <summary>Adds a comment to the task.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Add(Guid taskId, [FromBody] AddCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddCommentCommand(taskId, request.Content), cancellationToken);
        return result.IsSuccess
            ? Created($"{Request.Path}/{result.Value.Id}", result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");
    }

    /// <summary>Edits a comment. Author or Admin/Owner only.</summary>
    [HttpPut("{commentId:guid}")]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Edit(Guid taskId, Guid commentId, [FromBody] EditCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new EditCommentCommand(taskId, commentId, request.Content), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFoundProblem(result.Error);
    }

    /// <summary>Soft-deletes a comment. Author or Admin/Owner only.</summary>
    [HttpDelete("{commentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid taskId, Guid commentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCommentCommand(taskId, commentId), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFoundProblem(result.Error);
    }

    private IActionResult NotFoundProblem(Error error) =>
        error == TaskErrors.CommentNotFound
            ? ToProblem(error, StatusCodes.Status404NotFound, "Not Found", "comment-not-found")
            : ToProblem(error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");

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
