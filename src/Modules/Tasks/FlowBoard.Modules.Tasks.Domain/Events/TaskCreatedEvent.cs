using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Tasks.Domain.Events;

/// <summary>Raised when a task is created via <c>TaskItem.Create</c>.</summary>
/// <param name="TaskId">The new task.</param>
/// <param name="ProjectId">The project the task belongs to.</param>
/// <param name="OrganisationId">The organisation the task belongs to.</param>
/// <param name="CreatedById">The user who created the task.</param>
public sealed record TaskCreatedEvent(
    TaskId TaskId,
    ProjectId ProjectId,
    OrganisationId OrganisationId,
    UserId CreatedById) : DomainEvent;
