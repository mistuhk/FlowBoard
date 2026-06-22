using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.UpdateTaskDetails;

/// <summary>Handles <see cref="UpdateTaskDetailsCommand"/>: loads the task within the current organisation and updates it.</summary>
public sealed class UpdateTaskDetailsCommandHandler(
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<UpdateTaskDetailsCommand, Result<TaskResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<TaskResponse>> Handle(UpdateTaskDetailsCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<TaskResponse>(TaskErrors.NotFound);

        task.UpdateDetails(request.Title, request.Description, request.DueDate);

        return Result.Success(TaskResponse.From(task));
    }
}
