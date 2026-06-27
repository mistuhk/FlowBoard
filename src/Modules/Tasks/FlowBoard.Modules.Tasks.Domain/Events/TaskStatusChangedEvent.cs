using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Events;

/// <summary>
/// Raised when a task's status changes via <c>TaskItem.ChangeStatus</c>. Statuses are carried by
/// name so the payload serialises cleanly to the outbox. Consumed by ActivityLog.
/// </summary>
/// <param name="TaskId">The task whose status changed.</param>
/// <param name="OrganisationId">The organisation the task belongs to.</param>
/// <param name="OldStatus">The previous status name.</param>
/// <param name="NewStatus">The new status name.</param>
/// <param name="ChangedById">The user who changed the status.</param>
public sealed record TaskStatusChangedEvent(
    TaskId TaskId,
    OrganisationId OrganisationId,
    string OldStatus,
    string NewStatus,
    UserId ChangedById) : DomainEvent;
