using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskStatus;

/// <summary>
/// Handles <see cref="ChangeTaskStatusCommand"/>: loads the task within the current organisation and
/// applies the status change. The aggregate enforces the state machine (invalid transitions throw
/// and surface as 422).
/// </summary>
public sealed class ChangeTaskStatusCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<ChangeTaskStatusCommand, Result<TaskResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<TaskResponse>> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<TaskResponse>(TaskErrors.NotFound);

        task.ChangeStatus(TaskItemStatus.FromName(request.Status), currentUser.UserId);

        return Result.Success(TaskResponse.From(task));
    }
}
