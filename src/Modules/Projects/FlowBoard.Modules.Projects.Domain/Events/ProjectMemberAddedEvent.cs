using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Projects.Domain.Events;

/// <summary>
/// Raised when a user is added as a member of a project via <c>Project.AddMember</c>.
/// </summary>
/// <param name="ProjectId">The project the user was added to.</param>
/// <param name="OrganisationId">The organisation the project belongs to.</param>
/// <param name="UserId">The user who was added.</param>
public sealed record ProjectMemberAddedEvent(
    ProjectId ProjectId,
    OrganisationId OrganisationId,
    UserId UserId) : DomainEvent;
