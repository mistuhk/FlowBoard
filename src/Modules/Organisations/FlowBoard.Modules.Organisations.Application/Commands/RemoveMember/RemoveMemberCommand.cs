using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.RemoveMember;

/// <summary>
/// Removes a member from an organisation. The Owner cannot be removed, and the caller must
/// outrank the member being removed.
/// </summary>
/// <param name="OrganisationId">The organisation the member belongs to.</param>
/// <param name="UserId">The member to remove.</param>
public sealed record RemoveMemberCommand(Guid OrganisationId, Guid UserId)
    : ICommand<Result>;
