using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.DeleteComment;

/// <summary>Validates <see cref="DeleteCommentCommand"/>.</summary>
public sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    /// <summary>Configures the validation rules for deleting a comment.</summary>
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty().WithMessage("Task id is required.");
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("Comment id is required.");
    }
}
