using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Projects.Application.Commands.AddProjectMember;

/// <summary>
/// Handles <see cref="AddProjectMemberCommand"/>: loads the project within the current organisation
/// and adds the user as an explicit member. The user must already be a member of the organisation.
/// </summary>
public sealed class AddProjectMemberCommandHandler(
    ITenantContext tenantContext,
    IProjectRepository projects,
    IOrganisationMembershipReader membershipReader)
    : IRequestHandler<AddProjectMemberCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(
            ProjectId.From(request.ProjectId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (project is null)
            return Result.Failure(ProjectErrors.NotFound);

        var targetUserId = UserId.From(request.UserId);

        var roleName = await membershipReader.GetRoleNameAsync(
            tenantContext.CurrentOrganisationId, targetUserId, cancellationToken);
        if (roleName is null)
            return Result.Failure(ProjectErrors.UserNotOrganisationMember);

        project.AddMember(targetUserId);

        return Result.Success();
    }
}
