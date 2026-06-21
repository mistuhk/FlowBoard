using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.AcceptInvitation;

/// <summary>
/// Accepts an invitation using its single-use token, joining the authenticated user to the
/// organisation in the invited role.
/// </summary>
/// <param name="Token">The raw invitation token presented by the invitee.</param>
public sealed record AcceptInvitationCommand(string Token)
    : ICommand<Result<OrganisationResponse>>;
