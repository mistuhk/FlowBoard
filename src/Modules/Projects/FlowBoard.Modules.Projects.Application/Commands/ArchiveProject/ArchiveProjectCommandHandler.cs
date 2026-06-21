using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Commands.ArchiveProject;

/// <summary>
/// Handles <see cref="ArchiveProjectCommand"/>: loads the project within the current organisation
/// and archives it.
/// </summary>
public sealed class ArchiveProjectCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IProjectRepository projects)
    : IRequestHandler<ArchiveProjectCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        project.Archive(currentUser.UserId);

        return Result.Success();
    }
}
