using FlowBoard.Modules.ActivityLog.Application.Commands.LogActivity;
using FlowBoard.Modules.Projects.Domain.Events;
using MediatR;

namespace FlowBoard.Modules.ActivityLog.Application.EventHandlers;

/// <summary>Logs project creation.</summary>
public sealed class ProjectCreatedActivityHandler(ISender sender) : ActivityLogHandler<ProjectCreatedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(ProjectCreatedEvent e) =>
        new(e.OrganisationId.Value, "Project", e.ProjectId.Value, "project.created", e.CreatedById.Value, null);
}

/// <summary>Logs a project being archived.</summary>
public sealed class ProjectArchivedActivityHandler(ISender sender) : ActivityLogHandler<ProjectArchivedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(ProjectArchivedEvent e) =>
        new(e.OrganisationId.Value, "Project", e.ProjectId.Value, "project.archived", e.ArchivedById.Value, null);
}

/// <summary>Logs a project being restored.</summary>
public sealed class ProjectRestoredActivityHandler(ISender sender) : ActivityLogHandler<ProjectRestoredEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(ProjectRestoredEvent e) =>
        new(e.OrganisationId.Value, "Project", e.ProjectId.Value, "project.restored", null, null);
}

/// <summary>Logs a project deletion.</summary>
public sealed class ProjectDeletedActivityHandler(ISender sender) : ActivityLogHandler<ProjectDeletedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(ProjectDeletedEvent e) =>
        new(e.OrganisationId.Value, "Project", e.ProjectId.Value, "project.deleted", null, null);
}

/// <summary>Logs a member being added to a project.</summary>
public sealed class ProjectMemberAddedActivityHandler(ISender sender) : ActivityLogHandler<ProjectMemberAddedEvent>(sender)
{
    /// <inheritdoc/>
    protected override LogActivityCommand Map(ProjectMemberAddedEvent e) =>
        new(e.OrganisationId.Value, "Project", e.ProjectId.Value, "project.member_added", null,
            new Dictionary<string, object> { ["userId"] = e.UserId.Value });
}
