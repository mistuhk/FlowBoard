using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Projects.Domain.Events;

/// <summary>
/// Raised when a project is created via <c>Project.Create</c>. Consumed by other modules
/// (for example ActivityLog).
/// </summary>
/// <param name="ProjectId">The new project.</param>
/// <param name="OrganisationId">The organisation the project belongs to.</param>
/// <param name="CreatedById">The user who created the project.</param>
public sealed record ProjectCreatedEvent(
    ProjectId ProjectId,
    OrganisationId OrganisationId,
    UserId CreatedById) : DomainEvent;
