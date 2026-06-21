using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Commands.UpdateProject;

/// <summary>
/// Updates a project's editable details. Rejected with 422 while the project is archived.
/// </summary>
/// <param name="ProjectId">The project to update.</param>
/// <param name="Name">The new name. Between 1 and 150 characters.</param>
/// <param name="Description">The new description.</param>
public sealed record UpdateProjectCommand(Guid ProjectId, string Name, string? Description)
    : ICommand<Result<ProjectResponse>>;
