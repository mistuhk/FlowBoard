using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application;

/// <summary>Well-known <see cref="Error"/> values returned by Tasks handlers.</summary>
public static class TaskErrors
{
    /// <summary>Returned when the requested task does not exist in the caller's organisation.</summary>
    public static readonly Error NotFound = new(
        "Tasks.NotFound",
        "The task could not be found.");

    /// <summary>Returned when creating a task in a project that does not exist in the organisation.</summary>
    public static readonly Error ProjectNotFound = new(
        "Tasks.ProjectNotFound",
        "The project could not be found.");

    /// <summary>Returned when creating a task in an archived project.</summary>
    public static readonly Error ProjectArchived = new(
        "Tasks.ProjectArchived",
        "Tasks cannot be created in an archived project.");

    /// <summary>Returned when assigning a task to someone who is not a member of the organisation.</summary>
    public static readonly Error AssigneeNotOrganisationMember = new(
        "Tasks.AssigneeNotOrganisationMember",
        "The assignee must be a member of the organisation.");

    /// <summary>Returned when the requested comment does not exist on the task.</summary>
    public static readonly Error CommentNotFound = new(
        "Tasks.CommentNotFound",
        "The comment could not be found.");
}
