using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.Projects.Application.Queries.ListProjects;

/// <summary>Lists the active projects in the caller's current organisation.</summary>
public sealed record ListProjectsQuery : IQuery<IReadOnlyList<ProjectResponse>>;
