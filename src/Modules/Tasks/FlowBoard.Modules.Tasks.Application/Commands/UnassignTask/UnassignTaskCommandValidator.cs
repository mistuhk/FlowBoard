using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.UnassignTask;

/// <summary>Validates <see cref="UnassignTaskCommand"/>.</summary>
public sealed class UnassignTaskCommandValidator : AbstractValidator<UnassignTaskCommand>
{
    /// <summary>Configures the validation rules for unassigning a task.</summary>
    public UnassignTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task id is required.");
    }
}
