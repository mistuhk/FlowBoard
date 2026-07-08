namespace FlowBoard.Modules.Organisations.Application;

/// <summary>
/// The result of issuing an invitation. Carries the raw single-use token so the caller can
/// deliver the accept link to the invitee; the token is never persisted in raw form.
/// </summary>
/// <param name="InvitationId">The identifier of the created invitation.</param>
/// <param name="OrganisationId">The organisation the invitation is for.</param>
/// <param name="InvitedEmail">The normalised email address invited.</param>
/// <param name="Role">The name of the role the invitee will hold once accepted.</param>
/// <param name="ExpiresAt">UTC timestamp after which the invitation can no longer be accepted.</param>
/// <param name="Token">The raw single-use token to deliver to the invitee.</param>
public sealed record InvitationResponse(
    Guid InvitationId,
    Guid OrganisationId,
    string InvitedEmail,
    string Role,
    DateTime ExpiresAt,
    string Token);
