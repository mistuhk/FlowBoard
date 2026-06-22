using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskPriority;

/// <summary>Changes a task's priority.</summary>
/// <param name="TaskId">The task to change.</param>
/// <param name="Priority">The new priority (Low, Medium, High, or Critical).</param>
public sealed record ChangeTaskPriorityCommand(Guid TaskId, string Priority) : ICommand<Result>;
