using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.AddComment;

/// <summary>Validates <see cref="AddCommentCommand"/>.</summary>
public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    /// <summary>Configures the validation rules for adding a comment.</summary>
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty().WithMessage("Task id is required.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(10_000).WithMessage("Comment content must not exceed 10,000 characters.");
    }
}
