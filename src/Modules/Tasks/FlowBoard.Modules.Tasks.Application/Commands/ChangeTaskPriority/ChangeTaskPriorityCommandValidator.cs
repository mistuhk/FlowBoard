using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.ChangeTaskPriority;

/// <summary>Validates <see cref="ChangeTaskPriorityCommand"/>.</summary>
public sealed class ChangeTaskPriorityCommandValidator : AbstractValidator<ChangeTaskPriorityCommand>
{
    private static readonly string[] Priorities = ["low", "medium", "high", "critical"];

    /// <summary>Configures the validation rules for changing a task's priority.</summary>
    public ChangeTaskPriorityCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task id is required.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(p => Priorities.Contains(p.Trim().ToLowerInvariant()))
            .WithMessage("Priority must be one of Low, Medium, High, or Critical.");
    }
}
