using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.UpdateTaskDetails;

/// <summary>Validates <see cref="UpdateTaskDetailsCommand"/>.</summary>
public sealed class UpdateTaskDetailsCommandValidator : AbstractValidator<UpdateTaskDetailsCommand>
{
    /// <summary>Configures the validation rules for updating a task's details.</summary>
    public UpdateTaskDetailsCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task id is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");
    }
}
