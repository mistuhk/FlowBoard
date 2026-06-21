using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Queries.GetProject;

/// <summary>Returns a single project within the caller's current organisation.</summary>
/// <param name="ProjectId">The project to fetch.</param>
public sealed record GetProjectQuery(Guid ProjectId) : IQuery<Result<ProjectResponse>>;
