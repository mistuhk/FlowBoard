using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.DeleteTask;

/// <summary>Validates <see cref="DeleteTaskCommand"/>.</summary>
public sealed class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    /// <summary>Configures the validation rules for deleting a task.</summary>
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task id is required.");
    }
}
