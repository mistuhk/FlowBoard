using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Commands.RestoreProject;

/// <summary>Restores an archived project, making it editable again.</summary>
/// <param name="ProjectId">The project to restore.</param>
public sealed record RestoreProjectCommand(Guid ProjectId) : ICommand<Result>;
