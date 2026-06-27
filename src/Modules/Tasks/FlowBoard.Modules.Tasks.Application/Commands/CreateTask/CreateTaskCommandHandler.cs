using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Commands.CreateTask;

/// <summary>
/// Handles <see cref="CreateTaskCommand"/>: validates that the project exists in the current
/// organisation and is active, then creates the task with the caller as creator.
/// </summary>
public sealed class CreateTaskCommandHandler(
    ICurrentUserService currentUser,
    ITenantContext tenantContext,
    IProjectReader projectReader,
    ITaskRepository tasks)
    : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var organisationId = tenantContext.CurrentOrganisationId;
        var projectId = ProjectId.From(request.ProjectId);

        var availability = await projectReader.GetAvailabilityAsync(projectId, organisationId, cancellationToken);
        switch (availability)
        {
            case ProjectAvailability.NotFound:
                return Result.Failure<TaskResponse>(TaskErrors.ProjectNotFound);
            case ProjectAvailability.Archived:
                return Result.Failure<TaskResponse>(TaskErrors.ProjectArchived);
        }

        var task = TaskItem.Create(
            projectId,
            organisationId,
            request.Title,
            request.Description,
            Priority.FromPersistence(request.Priority),
            currentUser.UserId);

        await tasks.AddAsync(task, cancellationToken);

        return Result.Success(TaskResponse.From(task));
    }
}
