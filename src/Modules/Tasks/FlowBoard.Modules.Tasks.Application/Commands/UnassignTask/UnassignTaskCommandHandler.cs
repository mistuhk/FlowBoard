using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.UnassignTask;

/// <summary>Handles <see cref="UnassignTaskCommand"/>: loads the task within the current organisation and unassigns it.</summary>
public sealed class UnassignTaskCommandHandler(
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<UnassignTaskCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(UnassignTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        task.Unassign();

        return Result.Success();
    }
}
