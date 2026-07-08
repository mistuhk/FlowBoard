using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Commands.RemoveAttachment;

/// <summary>Validates <see cref="RemoveAttachmentCommand"/>.</summary>
public sealed class RemoveAttachmentCommandValidator : AbstractValidator<RemoveAttachmentCommand>
{
    /// <summary>Configures the validation rules for removing an attachment.</summary>
    public RemoveAttachmentCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty().WithMessage("Task id is required.");
        RuleFor(x => x.AttachmentId).NotEmpty().WithMessage("Attachment id is required.");
    }
}
