using FlowBoard.Modules.Tasks.Domain.Aggregates;

namespace FlowBoard.Modules.Tasks.Application;

/// <summary>The public representation of a task returned to API callers.</summary>
/// <param name="Id">The task id.</param>
/// <param name="ProjectId">The owning project.</param>
/// <param name="OrganisationId">The owning organisation.</param>
/// <param name="Title">The task title.</param>
/// <param name="Description">The task description, if any.</param>
/// <param name="Status">The status name (Todo, InProgress, Blocked, Done).</param>
/// <param name="Priority">The priority name (Low, Medium, High, Critical).</param>
/// <param name="AssigneeId">The assignee, if any.</param>
/// <param name="CreatedById">The user who created the task.</param>
/// <param name="DueDate">The due date, if any.</param>
/// <param name="CreatedAt">When the task was created.</param>
public sealed record TaskResponse(
    Guid Id,
    Guid ProjectId,
    Guid OrganisationId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    Guid? AssigneeId,
    Guid CreatedById,
    DateTime? DueDate,
    DateTime CreatedAt)
{
    /// <summary>Maps a <see cref="TaskItem"/> aggregate to its response representation.</summary>
    /// <param name="task">The task to map.</param>
    public static TaskResponse From(TaskItem task) =>
        new(
            task.Id.Value,
            task.ProjectId.Value,
            task.OrganisationId.Value,
            task.Title,
            task.Description,
            task.Status.Name,
            task.Priority.Name,
            task.AssigneeId?.Value,
            task.CreatedById.Value,
            task.DueDate,
            task.CreatedAt);
}
