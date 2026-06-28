using FlowBoard.Modules.ActivityLog.Application.Commands.LogActivity;
using FlowBoard.Modules.Tasks.Domain.Events;
using MediatR;

namespace FlowBoard.Modules.ActivityLog.Application.EventHandlers;

/// <summary>Logs task creation.</summary>
public sealed class TaskCreatedActivityHandler(ISender sender) : ActivityLogHandler<TaskCreatedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(TaskCreatedEvent e) =>
        new(e.OrganisationId.Value, "Task", e.TaskId.Value, "task.created", e.CreatedById.Value,
            new Dictionary<string, object> { ["projectId"] = e.ProjectId.Value });
}

/// <summary>Logs a task status change.</summary>
public sealed class TaskStatusChangedActivityHandler(ISender sender) : ActivityLogHandler<TaskStatusChangedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(TaskStatusChangedEvent e) =>
        new(e.OrganisationId.Value, "Task", e.TaskId.Value, "task.status_changed", e.ChangedById.Value,
            new Dictionary<string, object> { ["oldStatus"] = e.OldStatus, ["newStatus"] = e.NewStatus });
}

/// <summary>Logs a task assignment.</summary>
public sealed class TaskAssignedActivityHandler(ISender sender) : ActivityLogHandler<TaskAssignedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(TaskAssignedEvent e) =>
        new(e.OrganisationId.Value, "Task", e.TaskId.Value, "task.assigned", e.AssignedById.Value,
            new Dictionary<string, object> { ["assigneeId"] = e.AssigneeId.Value });
}

/// <summary>Logs a task being unassigned.</summary>
public sealed class TaskUnassignedActivityHandler(ISender sender) : ActivityLogHandler<TaskUnassignedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(TaskUnassignedEvent e) =>
        new(e.OrganisationId.Value, "Task", e.TaskId.Value, "task.unassigned", null,
            new Dictionary<string, object> { ["previousAssigneeId"] = e.PreviousAssigneeId.Value });
}

/// <summary>Logs a task priority change.</summary>
public sealed class TaskPriorityChangedActivityHandler(ISender sender) : ActivityLogHandler<TaskPriorityChangedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(TaskPriorityChangedEvent e) =>
        new(e.OrganisationId.Value, "Task", e.TaskId.Value, "task.priority_changed", null,
            new Dictionary<string, object> { ["oldPriority"] = e.OldPriority, ["newPriority"] = e.NewPriority });
}

/// <summary>Logs a task deletion.</summary>
public sealed class TaskDeletedActivityHandler(ISender sender) : ActivityLogHandler<TaskDeletedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(TaskDeletedEvent e) =>
        new(e.OrganisationId.Value, "Task", e.TaskId.Value, "task.deleted", null,
            new Dictionary<string, object> { ["projectId"] = e.ProjectId.Value });
}

/// <summary>Logs a comment being added to a task.</summary>
public sealed class CommentAddedActivityHandler(ISender sender) : ActivityLogHandler<CommentAddedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(CommentAddedEvent e) =>
        new(e.OrganisationId.Value, "Comment", e.CommentId.Value, "comment.added", e.AuthorId.Value,
            new Dictionary<string, object> { ["taskId"] = e.TaskId.Value });
}
