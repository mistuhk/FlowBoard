using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Commands.CreateProject;

/// <summary>
/// Creates a project in the caller's current organisation (resolved from the route). Restricted to
/// Admins and the Owner by the controller's authorisation policy.
/// </summary>
/// <param name="Name">The project name. Between 1 and 150 characters.</param>
/// <param name="Description">An optional description.</param>
public sealed record CreateProjectCommand(string Name, string? Description)
    : ICommand<Result<ProjectResponse>>;
