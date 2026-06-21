using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Application.Authorization;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Queries.GetProjectMembers;

/// <summary>
/// Handles <see cref="GetProjectMembersQuery"/>: lists a project's explicit members, scoped to the
/// current organisation. A Guest who is not a member of the project sees the project as not found.
/// </summary>
public sealed class GetProjectMembersQueryHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IProjectRepository projects,
    IOrganisationMembershipReader membershipReader)
    : IRequestHandler<GetProjectMembersQuery, Result<IReadOnlyList<ProjectMemberResponse>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ProjectMemberResponse>>> Handle(
        GetProjectMembersQuery request,
        CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (project is null || await GuestAccess.IsHiddenFromAsync(
                project, currentUser.UserId, tenantContext.CurrentOrganisationId, membershipReader, cancellationToken))
            return Result.Failure<IReadOnlyList<ProjectMemberResponse>>(ProjectErrors.NotFound);

        IReadOnlyList<ProjectMemberResponse> members =
            project.Members.Select(ProjectMemberResponse.From).ToList();

        return Result.Success(members);
    }
}
