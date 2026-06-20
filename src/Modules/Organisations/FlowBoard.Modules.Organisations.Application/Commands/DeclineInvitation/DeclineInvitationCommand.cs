using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.DeclineInvitation;

/// <summary>
/// Declines an invitation using its single-use token, removing the pending invitation.
/// </summary>
/// <param name="Token">The raw invitation token presented by the invitee.</param>
public sealed record DeclineInvitationCommand(string Token)
    : ICommand<Result>;
