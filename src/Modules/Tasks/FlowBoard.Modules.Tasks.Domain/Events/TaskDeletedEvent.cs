using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Events;

/// <summary>Raised when a task is soft-deleted via <c>TaskItem.Delete</c>.</summary>
/// <param name="TaskId">The deleted task.</param>
/// <param name="ProjectId">The project the task belonged to.</param>
/// <param name="OrganisationId">The organisation the task belonged to.</param>
public sealed record TaskDeletedEvent(
    TaskId TaskId,
    ProjectId ProjectId,
    OrganisationId OrganisationId) : DomainEvent;
