using FlowBoard.Application.Authorization;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Tasks.Application;
using FlowBoard.Modules.Tasks.Application.Commands.AssignTask;
using FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskPriority;
using FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskStatus;
using FlowBoard.Modules.Tasks.Application.Commands.CreateTask;
using FlowBoard.Modules.Tasks.Application.Commands.DeleteTask;
using FlowBoard.Modules.Tasks.Application.Commands.UnassignTask;
using FlowBoard.Modules.Tasks.Application.Commands.UpdateTaskDetails;
using FlowBoard.Modules.Tasks.Application.Queries.GetTasks;
using FlowBoard.Modules.Tasks.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Tasks.Presentation.Controllers;

/// <summary>
/// Task endpoints under <c>/api/v1/organisations/{orgId}/projects/{projectId}/tasks</c>. All require
/// organisation membership; access is scoped to the route organisation.
/// </summary>
[ApiController]
[Authorize(Policy = OrganisationPolicies.Member)]
[Route("api/v1/organisations/{orgId:guid}/projects/{projectId:guid}/tasks")]
public sealed class TasksController(ISender sender) : ControllerBase
{
    /// <summary>Lists tasks in the project, newest first, with optional filters and cursor pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(TaskPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid projectId,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] Guid? assigneeId,
        [FromQuery] DateTime? dueBefore,
        [FromQuery] DateTime? dueAfter,
        [FromQuery] string? cursor,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var page = await sender.Send(
            new GetTasksQuery(projectId, status, priority, assigneeId, dueBefore, dueAfter, cursor, limit),
            cancellationToken);

        return Ok(page);
    }

    /// <summary>Creates a task in the project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTaskCommand(projectId, request.Title, request.Description, request.Priority),
            cancellationToken);

        if (result.IsSuccess)
            return Created(
                $"/api/v1/organisations/{result.Value.OrganisationId}/projects/{projectId}/tasks/{result.Value.Id}",
                result.Value);

        return result.Error == TaskErrors.ProjectArchived
            ? ToProblem(result.Error, StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity", "project-archived")
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "project-not-found");
    }

    /// <summary>Updates a task's details.</summary>
    [HttpPut("{taskId:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid taskId,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTaskDetailsCommand(taskId, request.Title, request.Description, request.DueDate),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");
    }

    /// <summary>Changes a task's status (subject to the state machine; invalid transitions return 422).</summary>
    [HttpPut("{taskId:guid}/status")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeStatus(
        Guid taskId,
        [FromBody] ChangeTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeTaskStatusCommand(taskId, request.Status), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");
    }

    /// <summary>Assigns the task to a user.</summary>
    [HttpPut("{taskId:guid}/assignee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(
        Guid taskId,
        [FromBody] AssignTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AssignTaskCommand(taskId, request.AssigneeId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");
    }

    /// <summary>Removes the task's assignee.</summary>
    [HttpDelete("{taskId:guid}/assignee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnassignTaskCommand(taskId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");
    }

    /// <summary>Changes a task's priority.</summary>
    [HttpPut("{taskId:guid}/priority")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePriority(
        Guid taskId,
        [FromBody] ChangeTaskPriorityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeTaskPriorityCommand(taskId, request.Priority), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");
    }

    /// <summary>Soft-deletes a task.</summary>
    [HttpDelete("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTaskCommand(taskId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToProblem(result.Error, StatusCodes.Status404NotFound, "Not Found", "task-not-found");
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
