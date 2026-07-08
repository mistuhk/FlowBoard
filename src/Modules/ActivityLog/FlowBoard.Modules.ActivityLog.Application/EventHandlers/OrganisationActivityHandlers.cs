using FlowBoard.Modules.ActivityLog.Application.Commands.LogActivity;
using FlowBoard.Modules.Organisations.Domain.Events;
using MediatR;

namespace FlowBoard.Modules.ActivityLog.Application.EventHandlers;

/// <summary>Logs organisation creation.</summary>
public sealed class OrganisationCreatedActivityHandler(ISender sender) : ActivityLogHandler<OrganisationCreatedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(OrganisationCreatedEvent e) =>
        new(e.OrganisationId.Value, "Organisation", e.OrganisationId.Value, "organisation.created", e.OwnerId.Value, null);
}

/// <summary>Logs a member joining an organisation.</summary>
public sealed class MemberJoinedActivityHandler(ISender sender) : ActivityLogHandler<MemberJoinedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(MemberJoinedEvent e) =>
        new(e.OrganisationId.Value, "Membership", e.UserId.Value, "member.joined", e.UserId.Value,
            new Dictionary<string, object> { ["role"] = e.Role });
}

/// <summary>Logs a member being removed from an organisation.</summary>
public sealed class MemberRemovedActivityHandler(ISender sender) : ActivityLogHandler<MemberRemovedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(MemberRemovedEvent e) =>
        new(e.OrganisationId.Value, "Membership", e.UserId.Value, "member.removed", e.RemovedById.Value, null);
}

/// <summary>Logs a member's role changing.</summary>
public sealed class MemberRoleChangedActivityHandler(ISender sender) : ActivityLogHandler<MemberRoleChangedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(MemberRoleChangedEvent e) =>
        new(e.OrganisationId.Value, "Membership", e.UserId.Value, "member.role_changed", null,
            new Dictionary<string, object> { ["oldRole"] = e.OldRole, ["newRole"] = e.NewRole });
}

/// <summary>Logs ownership of an organisation being transferred.</summary>
public sealed class OwnershipTransferredActivityHandler(ISender sender) : ActivityLogHandler<OwnershipTransferredEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(OwnershipTransferredEvent e) =>
        new(e.OrganisationId.Value, "Organisation", e.OrganisationId.Value, "organisation.ownership_transferred", e.FromUserId.Value,
            new Dictionary<string, object> { ["fromUserId"] = e.FromUserId.Value, ["toUserId"] = e.ToUserId.Value });
}
