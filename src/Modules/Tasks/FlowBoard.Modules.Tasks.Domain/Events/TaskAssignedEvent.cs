using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Events;

/// <summary>
/// Raised when a task is assigned via <c>TaskItem.Assign</c>. Consumed by the Notifications module
/// to notify the assignee.
/// </summary>
/// <param name="TaskId">The assigned task.</param>
/// <param name="OrganisationId">The organisation the task belongs to.</param>
/// <param name="AssigneeId">The user the task was assigned to.</param>
/// <param name="AssignedById">The user who made the assignment.</param>
public sealed record TaskAssignedEvent(
    TaskId TaskId,
    OrganisationId OrganisationId,
    UserId AssigneeId,
    UserId AssignedById) : DomainEvent;
