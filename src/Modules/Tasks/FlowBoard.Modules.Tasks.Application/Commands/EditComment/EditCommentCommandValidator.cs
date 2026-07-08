using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.EditComment;

/// <summary>Validates <see cref="EditCommentCommand"/>.</summary>
public sealed class EditCommentCommandValidator : AbstractValidator<EditCommentCommand>
{
    /// <summary>Configures the validation rules for editing a comment.</summary>
    public EditCommentCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty().WithMessage("Task id is required.");
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("Comment id is required.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(10_000).WithMessage("Comment content must not exceed 10,000 characters.");
    }
}
