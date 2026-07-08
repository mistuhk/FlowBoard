using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.AssignTask;

/// <summary>Assigns a task to a user.</summary>
/// <param name="TaskId">The task to assign.</param>
/// <param name="AssigneeId">The user to assign it to.</param>
public sealed record AssignTaskCommand(Guid TaskId, Guid AssigneeId) : ICommand<Result>;
