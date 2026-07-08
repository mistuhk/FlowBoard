using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.CreateTask;

/// <summary>Validates <see cref="CreateTaskCommand"/>.</summary>
public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    private static readonly string[] Priorities = ["low", "medium", "high", "critical"];

    /// <summary>Configures the validation rules for creating a task.</summary>
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(p => Priorities.Contains(p.Trim().ToLowerInvariant()))
            .WithMessage("Priority must be one of Low, Medium, High, or Critical.");
    }
}
