using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.InviteMember;

/// <summary>
/// Invites a person (by email) to join an organisation in the given role. Only an Admin or the
/// Owner of the organisation may issue invitations.
/// </summary>
/// <param name="OrganisationId">The organisation to invite into.</param>
/// <param name="Email">The invitee's email address.</param>
/// <param name="Role">The role the invitee will hold once accepted. One of Admin, Member, or Guest.</param>
public sealed record InviteMemberCommand(Guid OrganisationId, string Email, string Role)
    : ICommand<Result<InvitationResponse>>;
