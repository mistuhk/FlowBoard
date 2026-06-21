using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Projects.Domain.Events;

/// <summary>
/// Raised when a project is archived via <c>Project.Archive</c>.
/// </summary>
/// <param name="ProjectId">The archived project.</param>
/// <param name="OrganisationId">The organisation the project belongs to.</param>
/// <param name="ArchivedById">The user who archived the project.</param>
public sealed record ProjectArchivedEvent(
    ProjectId ProjectId,
    OrganisationId OrganisationId,
    UserId ArchivedById) : DomainEvent;
