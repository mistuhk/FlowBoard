using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Repositories;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Commands.UpdateProject;

/// <summary>
/// Handles <see cref="UpdateProjectCommand"/>: loads the project within the current organisation and
/// applies the new details. The aggregate rejects edits to an archived project.
/// </summary>
public sealed class UpdateProjectCommandHandler(
    ITenantContext tenantContext,
    IProjectRepository projects)
    : IRequestHandler<UpdateProjectCommand, Result<ProjectResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<ProjectResponse>> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (project is null)
            return Result.Failure<ProjectResponse>(ProjectErrors.NotFound);

        project.Update(ProjectName.Create(request.Name), request.Description);

        return Result.Success(ProjectResponse.From(project));
    }
}
