using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Commands.RemoveProjectMember;

/// <summary>
/// Handles <see cref="RemoveProjectMemberCommand"/>: loads the project within the current
/// organisation and removes the explicit member.
/// </summary>
public sealed class RemoveProjectMemberCommandHandler(
    ITenantContext tenantContext,
    IProjectRepository projects)
    : IRequestHandler<RemoveProjectMemberCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        project.RemoveMember(UserId.From(request.UserId));

        return Result.Success();
    }
}
