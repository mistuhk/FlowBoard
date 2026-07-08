using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Projects.Domain.Aggregates;
using FlowBoard.Modules.Projects.Domain.Repositories;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Commands.CreateProject;

/// <summary>
/// Handles <see cref="CreateProjectCommand"/>: creates a project in the current tenant organisation
/// with the caller as creator. Tenant membership and the Admin/Owner gate are enforced by the
/// authorisation policy before the handler runs.
/// </summary>
public sealed class CreateProjectCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IProjectRepository projects)
    : IRequestHandler<CreateProjectCommand, Result<ProjectResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<ProjectResponse>> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = Project.Create(
            tenantContext.CurrentOrganisationId,
            ProjectName.Create(request.Name),
            request.Description,
            currentUser.UserId);

        await projects.AddAsync(project, cancellationToken);

        return Result.Success(ProjectResponse.From(project));
    }
}
