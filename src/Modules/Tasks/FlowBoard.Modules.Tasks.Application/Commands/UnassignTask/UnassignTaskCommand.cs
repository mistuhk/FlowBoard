using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.UnassignTask;

/// <summary>Removes a task's assignee.</summary>
/// <param name="TaskId">The task to unassign.</param>
public sealed record UnassignTaskCommand(Guid TaskId) : ICommand<Result>;
