namespace FlowBoard.Modules.Projects.Presentation.Contracts;

/// <summary>Request body for updating a project.</summary>
/// <param name="Name">The new name. Between 1 and 150 characters.</param>
/// <param name="Description">The new description.</param>
public sealed record UpdateProjectRequest(string Name, string? Description);
