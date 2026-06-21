namespace FlowBoard.Modules.Projects.Presentation.Contracts;

/// <summary>Request body for adding a project member.</summary>
/// <param name="UserId">The organisation member to add to the project.</param>
public sealed record AddProjectMemberRequest(Guid UserId);
