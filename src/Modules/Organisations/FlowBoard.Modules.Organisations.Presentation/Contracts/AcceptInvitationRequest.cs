namespace FlowBoard.Modules.Organisations.Presentation.Contracts;

/// <summary>
/// Request body for accepting an invitation.
/// </summary>
/// <param name="Token">The raw invitation token received by the invitee.</param>
public sealed record AcceptInvitationRequest(string Token);
