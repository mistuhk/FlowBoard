using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Events;

/// <summary>
/// Raised when a task's priority changes via <c>TaskItem.ChangePriority</c>. Priorities are carried
/// by name so the payload serialises cleanly to the outbox.
/// </summary>
/// <param name="TaskId">The task whose priority changed.</param>
/// <param name="OrganisationId">The organisation the task belongs to.</param>
/// <param name="OldPriority">The previous priority name.</param>
/// <param name="NewPriority">The new priority name.</param>
public sealed record TaskPriorityChangedEvent(
    TaskId TaskId,
    OrganisationId OrganisationId,
    string OldPriority,
    string NewPriority) : DomainEvent;
