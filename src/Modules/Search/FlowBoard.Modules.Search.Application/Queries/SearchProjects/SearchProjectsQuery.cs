using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.Search.Application.Queries.SearchProjects;

/// <summary>Full-text search over projects in the current organisation.</summary>
/// <param name="Query">The free-text query, or null/blank to list recent projects.</param>
/// <param name="Limit">Maximum results (clamped server-side).</param>
public sealed record SearchProjectsQuery(string? Query, int? Limit) : IQuery<IReadOnlyList<ProjectSearchResult>>;
