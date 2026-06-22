namespace FlowBoard.Modules.Tasks.Presentation.Contracts;

/// <summary>Request body for creating a task.</summary>
/// <param name="Title">The task title (1 to 255 characters).</param>
/// <param name="Description">An optional description.</param>
/// <param name="Priority">The initial priority (Low, Medium, High, or Critical).</param>
public sealed record CreateTaskRequest(string Title, string? Description, string Priority);

/// <summary>Request body for updating a task's details.</summary>
/// <param name="Title">The new title (1 to 255 characters).</param>
/// <param name="Description">The new description.</param>
/// <param name="DueDate">The new due date, or <c>null</c> to clear it.</param>
public sealed record UpdateTaskRequest(string Title, string? Description, DateTime? DueDate);

/// <summary>Request body for changing a task's status.</summary>
/// <param name="Status">The target status (Todo, InProgress, Blocked, or Done).</param>
public sealed record ChangeTaskStatusRequest(string Status);

/// <summary>Request body for assigning a task.</summary>
/// <param name="AssigneeId">The user to assign the task to.</param>
public sealed record AssignTaskRequest(Guid AssigneeId);

/// <summary>Request body for changing a task's priority.</summary>
/// <param name="Priority">The new priority (Low, Medium, High, or Critical).</param>
public sealed record ChangeTaskPriorityRequest(string Priority);
