using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Application.Authorization;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Queries.GetProject;

/// <summary>
/// Handles <see cref="GetProjectQuery"/>: loads a project scoped to the current organisation.
/// Read-only, so it runs outside a transaction. A project in another organisation, or one a Guest
/// has not been added to, is reported as not found.
/// </summary>
public sealed class GetProjectQueryHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IProjectRepository projects,
    IOrganisationMembershipReader membershipReader)
    : IRequestHandler<GetProjectQuery, Result<ProjectResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<ProjectResponse>> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (project is null || await GuestAccess.IsHiddenFromAsync(
                project, currentUser.UserId, tenantContext.CurrentOrganisationId, membershipReader, cancellationToken))
            return Result.Failure<ProjectResponse>(ProjectErrors.NotFound);

        return Result.Success(ProjectResponse.From(project));
    }
}
