using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Commands.DeleteProject;

/// <summary>Soft-deletes a project.</summary>
/// <param name="ProjectId">The project to delete.</param>
public sealed record DeleteProjectCommand(Guid ProjectId) : ICommand<Result>;
