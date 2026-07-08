using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.CreateTask;

/// <summary>Creates a task in a project within the caller's current organisation.</summary>
/// <param name="ProjectId">The project to create the task in.</param>
/// <param name="Title">The task title (1 to 255 characters).</param>
/// <param name="Description">An optional description.</param>
/// <param name="Priority">The initial priority (Low, Medium, High, or Critical).</param>
public sealed record CreateTaskCommand(Guid ProjectId, string Title, string? Description, string Priority)
    : ICommand<Result<TaskResponse>>;
