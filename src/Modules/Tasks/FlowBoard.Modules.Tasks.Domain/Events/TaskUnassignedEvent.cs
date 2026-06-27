using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Events;

/// <summary>Raised when a task is unassigned via <c>TaskItem.Unassign</c>.</summary>
/// <param name="TaskId">The task that was unassigned.</param>
/// <param name="OrganisationId">The organisation the task belongs to.</param>
/// <param name="PreviousAssigneeId">The user who was previously assigned.</param>
public sealed record TaskUnassignedEvent(
    TaskId TaskId,
    OrganisationId OrganisationId,
    UserId PreviousAssigneeId) : DomainEvent;
