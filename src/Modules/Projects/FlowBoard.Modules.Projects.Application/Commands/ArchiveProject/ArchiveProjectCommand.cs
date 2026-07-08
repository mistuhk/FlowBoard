using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Commands.ArchiveProject;

/// <summary>Archives a project, making it read-only.</summary>
/// <param name="ProjectId">The project to archive.</param>
public sealed record ArchiveProjectCommand(Guid ProjectId) : ICommand<Result>;
