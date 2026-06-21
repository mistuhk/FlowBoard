using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Commands.DeleteProject;

/// <summary>
/// Handles <see cref="DeleteProjectCommand"/>: loads the project within the current organisation and
/// soft-deletes it.
/// </summary>
public sealed class DeleteProjectCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IProjectRepository projects)
    : IRequestHandler<DeleteProjectCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        project.Delete(currentUser.UserId);

        return Result.Success();
    }
}
