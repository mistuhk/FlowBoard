using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.ActivityLog.Application.Commands.LogActivity;

/// <summary>
/// Appends an activity-log entry. Sent by the per-event activity handlers so the write rides the
/// command transaction pipeline, consistent with how other cross-module reactions persist.
/// </summary>
/// <param name="OrganisationId">The organisation the activity belongs to.</param>
/// <param name="EntityType">The kind of entity acted upon.</param>
/// <param name="EntityId">The id of the entity acted upon.</param>
/// <param name="EventType">The event token (for example <c>task.assigned</c>).</param>
/// <param name="ActorId">The acting user, or <c>null</c> if system-generated.</param>
/// <param name="Metadata">Optional additional context.</param>
public sealed record LogActivityCommand(
    Guid OrganisationId,
    string EntityType,
    Guid EntityId,
    string EventType,
    Guid? ActorId,
    Dictionary<string, object>? Metadata) : ICommand<Result>;
