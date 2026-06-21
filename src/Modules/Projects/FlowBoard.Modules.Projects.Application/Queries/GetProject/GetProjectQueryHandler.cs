using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Queries.GetProject;

/// <summary>
/// Handles <see cref="GetProjectQuery"/>: loads a project scoped to the current organisation.
/// Read-only, so it runs outside a transaction. A project in another organisation is reported as
/// not found.
/// </summary>
public sealed class GetProjectQueryHandler(
    ITenantContext tenantContext,
    IProjectRepository projects)
    : IRequestHandler<GetProjectQuery, Result<ProjectResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<ProjectResponse>> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        return project is null
            ? Result.Failure<ProjectResponse>(ProjectErrors.NotFound)
            : Result.Success(ProjectResponse.From(project));
    }
}
