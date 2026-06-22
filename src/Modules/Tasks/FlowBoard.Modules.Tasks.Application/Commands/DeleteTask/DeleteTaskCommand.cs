using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.DeleteTask;

/// <summary>Soft-deletes a task.</summary>
/// <param name="TaskId">The task to delete.</param>
public sealed record DeleteTaskCommand(Guid TaskId) : ICommand<Result>;
