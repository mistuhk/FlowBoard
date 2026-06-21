namespace FlowBoard.Modules.Projects.Presentation.Contracts;

/// <summary>Request body for creating a project.</summary>
/// <param name="Name">The project name. Between 1 and 150 characters.</param>
/// <param name="Description">An optional description.</param>
public sealed record CreateProjectRequest(string Name, string? Description);
