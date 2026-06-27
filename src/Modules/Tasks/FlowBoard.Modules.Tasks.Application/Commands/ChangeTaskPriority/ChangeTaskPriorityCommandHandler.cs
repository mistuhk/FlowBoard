using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskPriority;

/// <summary>Handles <see cref="ChangeTaskPriorityCommand"/>: loads the task within the current organisation and changes its priority.</summary>
public sealed class ChangeTaskPriorityCommandHandler(
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<ChangeTaskPriorityCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(ChangeTaskPriorityCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        task.ChangePriority(Priority.FromPersistence(request.Priority));

        return Result.Success();
    }
}
