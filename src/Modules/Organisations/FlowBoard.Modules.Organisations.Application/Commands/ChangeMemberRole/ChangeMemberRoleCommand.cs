using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.ChangeMemberRole;

/// <summary>
/// Changes a member's role within an organisation. The caller must outrank both the member's
/// current role and the new role; ownership cannot be assigned this way.
/// </summary>
/// <param name="OrganisationId">The organisation the member belongs to.</param>
/// <param name="UserId">The member whose role is changing.</param>
/// <param name="Role">The new role. One of Admin, Member, or Guest.</param>
public sealed record ChangeMemberRoleCommand(Guid OrganisationId, Guid UserId, string Role)
    : ICommand<Result>;
