using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.AssignTask;

/// <summary>
/// Handles <see cref="AssignTaskCommand"/>: loads the task within the current organisation and
/// assigns it. Validating that the assignee is an organisation member is added in US-017.
/// </summary>
public sealed class AssignTaskCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<AssignTaskCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        task.Assign(UserId.From(request.AssigneeId), currentUser.UserId);

        return Result.Success();
    }
}
