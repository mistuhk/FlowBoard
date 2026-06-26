using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.AssignTask;

/// <summary>
/// Handles <see cref="AssignTaskCommand"/>: loads the task within the current organisation, checks
/// the assignee is a member of that organisation, and assigns it.
/// </summary>
public sealed class AssignTaskCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks,
    IOrganisationMembershipReader membershipReader)
    : IRequestHandler<AssignTaskCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        var assigneeId = UserId.From(request.AssigneeId);

        var roleName = await membershipReader.GetRoleNameAsync(
            tenantContext.CurrentOrganisationId, assigneeId, cancellationToken);
        if (roleName is null)
            return Result.Failure(TaskErrors.AssigneeNotOrganisationMember);

        task.Assign(assigneeId, currentUser.UserId);

        return Result.Success();
    }
}
