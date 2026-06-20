namespace FlowBoard.Modules.Organisations.Presentation.Contracts;

/// <summary>
/// Request body for inviting a member to an organisation.
/// </summary>
/// <param name="Email">The invitee's email address.</param>
/// <param name="Role">The role the invitee will hold once accepted. One of Admin, Member, or Guest.</param>
public sealed record InviteMemberRequest(string Email, string Role);
