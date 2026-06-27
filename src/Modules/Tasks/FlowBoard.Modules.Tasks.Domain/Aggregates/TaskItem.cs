using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Events;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Aggregates;

/// <summary>
/// Aggregate root for the Tasks bounded context (named <c>TaskItem</c> to avoid colliding with
/// <see cref="System.Threading.Tasks.Task"/>). A task belongs to one project and one organisation,
/// both denormalised and fixed for its lifetime. Status changes follow a state machine; invalid
/// transitions throw <see cref="InvalidStatusTransitionException"/>.
/// </summary>
public sealed class TaskItem : AggregateRoot<TaskId>
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private TaskItem() { }

    /// <summary>The project the task belongs to. Fixed for the task's lifetime.</summary>
    public ProjectId ProjectId { get; private set; }

    /// <summary>The organisation the task belongs to (denormalised for tenant scoping).</summary>
    public OrganisationId OrganisationId { get; private set; }

    /// <summary>The task title (1 to 255 characters).</summary>
    public string Title { get; private set; } = null!;

    /// <summary>An optional free-text description.</summary>
    public string? Description { get; private set; }

    /// <summary>The task's status.</summary>
    public TaskItemStatus Status { get; private set; } = null!;

    /// <summary>The task's priority.</summary>
    public Priority Priority { get; private set; } = null!;

    /// <summary>The assignee, or <c>null</c> if unassigned.</summary>
    public UserId? AssigneeId { get; private set; }

    /// <summary>The user who created the task.</summary>
    public UserId CreatedById { get; private set; }

    /// <summary>An optional due date.</summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>
    /// UTC timestamp of the most recent change. Database-managed by the <c>updated_at</c> trigger;
    /// never assigned in code.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>UTC timestamp at which the task was soft-deleted. <c>null</c> if active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Creates a new task in the Todo status and raises <see cref="TaskCreatedEvent"/>.</summary>
    /// <param name="projectId">The owning project.</param>
    /// <param name="organisationId">The owning organisation.</param>
    /// <param name="title">The task title (1 to 255 characters).</param>
    /// <param name="description">An optional description.</param>
    /// <param name="priority">The initial priority.</param>
    /// <param name="createdById">The user creating the task.</param>
    /// <returns>A new <see cref="TaskItem"/>.</returns>
    public static TaskItem Create(
        ProjectId projectId,
        OrganisationId organisationId,
        string title,
        string? description,
        Priority priority,
        UserId createdById)
    {
        var task = new TaskItem
        {
            Id = TaskId.New(),
            ProjectId = projectId,
            OrganisationId = organisationId,
            Title = ValidateTitle(title),
            Description = description?.Trim(),
            Status = TaskItemStatus.Todo,
            Priority = priority,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow,  // object initialiser bypasses the base ctor
        };

        task.Raise(new TaskCreatedEvent(task.Id, projectId, organisationId, createdById));
        return task;
    }

    /// <summary>Updates the editable details.</summary>
    /// <param name="title">The new title (1 to 255 characters).</param>
    /// <param name="description">The new description.</param>
    /// <param name="dueDate">The new due date, or <c>null</c> to clear it.</param>
    public void UpdateDetails(string title, string? description, DateTime? dueDate)
    {
        Title = ValidateTitle(title);
        Description = description?.Trim();
        DueDate = dueDate;
    }

    /// <summary>
    /// Changes the status, enforcing the state machine, and raises <see cref="TaskStatusChangedEvent"/>.
    /// </summary>
    /// <param name="newStatus">The status to transition to.</param>
    /// <param name="changedById">The user making the change.</param>
    /// <exception cref="InvalidStatusTransitionException">Thrown if the transition is not allowed.</exception>
    public void ChangeStatus(TaskItemStatus newStatus, UserId changedById)
    {
        if (!Status.CanTransitionTo(newStatus))
            throw new InvalidStatusTransitionException(Status.Name, newStatus.Name);

        var oldStatus = Status;
        Status = newStatus;

        Raise(new TaskStatusChangedEvent(Id, OrganisationId, oldStatus.Name, newStatus.Name, changedById));
    }

    /// <summary>
    /// Assigns the task to a user and raises <see cref="TaskAssignedEvent"/>. The assignee must be a
    /// member of the organisation; that is validated in the application layer before this is called.
    /// </summary>
    /// <param name="assigneeId">The user to assign.</param>
    /// <param name="assignedById">The user making the assignment.</param>
    public void Assign(UserId assigneeId, UserId assignedById)
    {
        AssigneeId = assigneeId;
        Raise(new TaskAssignedEvent(Id, OrganisationId, assigneeId, assignedById));
    }

    /// <summary>
    /// Clears the assignee and raises <see cref="TaskUnassignedEvent"/>. A no-op if the task is
    /// already unassigned.
    /// </summary>
    public void Unassign()
    {
        if (AssigneeId is null)
            return;

        var previous = AssigneeId.Value;
        AssigneeId = null;

        Raise(new TaskUnassignedEvent(Id, OrganisationId, previous));
    }

    /// <summary>
    /// Changes the priority and raises <see cref="TaskPriorityChangedEvent"/>. A no-op if unchanged.
    /// </summary>
    /// <param name="newPriority">The new priority.</param>
    public void ChangePriority(Priority newPriority)
    {
        if (Priority == newPriority)
            return;

        var oldPriority = Priority;
        Priority = newPriority;

        Raise(new TaskPriorityChangedEvent(Id, OrganisationId, oldPriority.Name, newPriority.Name));
    }

    /// <summary>Soft-deletes the task and raises <see cref="TaskDeletedEvent"/>. Idempotent.</summary>
    /// <param name="deletedById">The user deleting the task.</param>
    public void Delete(UserId deletedById)
    {
        if (DeletedAt is not null)
            return;

        DeletedAt = DateTime.UtcNow;
        Raise(new TaskDeletedEvent(Id, ProjectId, OrganisationId));
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title cannot be empty.");

        var trimmed = title.Trim();

        return trimmed.Length > 255
            ? throw new DomainException("Task title must not exceed 255 characters.")
            : trimmed;
    }
}
