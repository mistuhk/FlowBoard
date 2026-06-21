using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Projects.Domain.Events;

/// <summary>
/// Raised when an archived project is restored via <c>Project.Restore</c>.
/// </summary>
/// <param name="ProjectId">The restored project.</param>
/// <param name="OrganisationId">The organisation the project belongs to.</param>
public sealed record ProjectRestoredEvent(
    ProjectId ProjectId,
    OrganisationId OrganisationId) : DomainEvent;
