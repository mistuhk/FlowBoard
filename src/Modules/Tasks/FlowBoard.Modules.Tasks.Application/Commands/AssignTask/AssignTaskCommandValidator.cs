using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.AssignTask;

/// <summary>Validates <see cref="AssignTaskCommand"/>.</summary>
public sealed class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
    /// <summary>Configures the validation rules for assigning a task.</summary>
    public AssignTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task id is required.");

        RuleFor(x => x.AssigneeId)
            .NotEmpty().WithMessage("Assignee id is required.");
    }
}
