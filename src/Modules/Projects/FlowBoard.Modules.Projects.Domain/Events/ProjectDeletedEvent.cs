using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Projects.Domain.Events;

/// <summary>
/// Raised when a project is soft-deleted via <c>Project.Delete</c>.
/// </summary>
/// <param name="ProjectId">The deleted project.</param>
/// <param name="OrganisationId">The organisation the project belonged to.</param>
public sealed record ProjectDeletedEvent(
    ProjectId ProjectId,
    OrganisationId OrganisationId) : DomainEvent;
