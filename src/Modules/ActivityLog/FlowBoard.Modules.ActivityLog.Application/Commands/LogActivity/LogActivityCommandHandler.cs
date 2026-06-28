using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.ActivityLog.Domain;
using MediatR;

namespace FlowBoard.Modules.ActivityLog.Application.Commands.LogActivity;

/// <summary>Handles <see cref="LogActivityCommand"/>: appends one immutable activity-log entry.</summary>
public sealed class LogActivityCommandHandler(IActivityLogRepository activityLog)
    : IRequestHandler<LogActivityCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(LogActivityCommand request, CancellationToken cancellationToken)
    {
        var entry = ActivityLogEntry.Create(
            OrganisationId.From(request.OrganisationId),
            request.EntityType,
            request.EntityId,
            request.EventType,
            request.ActorId.HasValue ? UserId.From(request.ActorId.Value) : null,
            request.Metadata);

        await activityLog.AddAsync(entry, cancellationToken);

        return Result.Success();
    }
}
