using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Projects.Application.Authorization;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Queries.ListProjects;

/// <summary>
/// Handles <see cref="ListProjectsQuery"/>: lists the active projects in the current organisation.
/// A Guest sees only the projects they have been added to; all other members see every project.
/// Read-only, so it runs outside a transaction.
/// </summary>
public sealed class ListProjectsQueryHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IProjectRepository projects,
    IOrganisationMembershipReader membershipReader)
    : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectResponse>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProjectResponse>> Handle(
        ListProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var found = await projects.ListByOrganisationAsync(
            tenantContext.CurrentOrganisationId, cancellationToken);

        if (await GuestAccess.IsGuestAsync(
                tenantContext.CurrentOrganisationId, currentUser.UserId, membershipReader, cancellationToken))
        {
            found = found.Where(p => p.HasMember(currentUser.UserId)).ToList();
        }

        return found.Select(ProjectResponse.From).ToList();
    }
}
