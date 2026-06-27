using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.UpdateTaskDetails;

/// <summary>Updates a task's editable details.</summary>
/// <param name="TaskId">The task to update.</param>
/// <param name="Title">The new title (1 to 255 characters).</param>
/// <param name="Description">The new description.</param>
/// <param name="DueDate">The new due date, or <c>null</c> to clear it.</param>
public sealed record UpdateTaskDetailsCommand(Guid TaskId, string Title, string? Description, DateTime? DueDate)
    : ICommand<Result<TaskResponse>>;
