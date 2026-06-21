namespace FlowBoard.Modules.Organisations.Presentation.Contracts;

/// <summary>
/// Request body for changing a member's role.
/// </summary>
/// <param name="Role">The new role. One of Admin, Member, or Guest.</param>
public sealed record ChangeMemberRoleRequest(string Role);
