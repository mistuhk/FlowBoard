using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskStatus;

/// <summary>Validates <see cref="ChangeTaskStatusCommand"/>.</summary>
public sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    private static readonly string[] Statuses = ["todo", "inprogress", "blocked", "done"];

    /// <summary>Configures the validation rules for changing a task's status.</summary>
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task id is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => Statuses.Contains(s.Trim().ToLowerInvariant()))
            .WithMessage("Status must be one of Todo, InProgress, Blocked, or Done.");
    }
}
