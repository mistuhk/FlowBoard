using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Queries.GetProjectMembers;

/// <summary>Lists the explicit members of a project.</summary>
/// <param name="ProjectId">The project whose members to list.</param>
public sealed record GetProjectMembersQuery(Guid ProjectId)
    : IQuery<Result<IReadOnlyList<ProjectMemberResponse>>>;
