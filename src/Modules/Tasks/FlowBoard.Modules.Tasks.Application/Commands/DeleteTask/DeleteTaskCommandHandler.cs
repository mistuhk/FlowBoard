using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.DeleteTask;

/// <summary>Handles <see cref="DeleteTaskCommand"/>: loads the task within the current organisation and soft-deletes it.</summary>
public sealed class DeleteTaskCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<DeleteTaskCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure(TaskErrors.NotFound);

        task.Delete(currentUser.UserId);

        return Result.Success();
    }
}
