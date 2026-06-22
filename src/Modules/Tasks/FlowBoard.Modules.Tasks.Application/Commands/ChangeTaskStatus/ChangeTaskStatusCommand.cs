using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskStatus;

/// <summary>Changes a task's status, subject to the state-machine transitions.</summary>
/// <param name="TaskId">The task to change.</param>
/// <param name="Status">The target status (Todo, InProgress, Blocked, or Done).</param>
public sealed record ChangeTaskStatusCommand(Guid TaskId, string Status)
    : ICommand<Result<TaskResponse>>;
