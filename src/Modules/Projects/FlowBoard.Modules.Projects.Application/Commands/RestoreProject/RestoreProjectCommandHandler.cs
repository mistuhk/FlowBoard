using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Commands.RestoreProject;

/// <summary>
/// Handles <see cref="RestoreProjectCommand"/>: loads the project within the current organisation
/// and restores it.
/// </summary>
public sealed class RestoreProjectCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IProjectRepository projects)
    : IRequestHandler<RestoreProjectCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RestoreProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        project.Restore(currentUser.UserId);

        return Result.Success();
    }
}
