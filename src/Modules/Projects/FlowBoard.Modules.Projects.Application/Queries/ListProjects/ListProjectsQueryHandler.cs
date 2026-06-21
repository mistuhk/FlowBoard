using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Queries.ListProjects;

/// <summary>
/// Handles <see cref="ListProjectsQuery"/>: lists the active projects in the current organisation.
/// Read-only, so it runs outside a transaction.
/// </summary>
public sealed class ListProjectsQueryHandler(
    ITenantContext tenantContext,
    IProjectRepository projects)
    : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectResponse>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProjectResponse>> Handle(
        ListProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var found = await projects.ListByOrganisationAsync(
            tenantContext.CurrentOrganisationId, cancellationToken);

        return found.Select(ProjectResponse.From).ToList();
    }
}
