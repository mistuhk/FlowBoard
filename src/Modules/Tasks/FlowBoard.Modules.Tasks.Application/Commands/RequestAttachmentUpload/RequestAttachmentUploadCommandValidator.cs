using FluentValidation;
using FlowBoard.Modules.Tasks.Application.Attachments;

namespace FlowBoard.Modules.Tasks.Application.Commands.RequestAttachmentUpload;

/// <summary>Validates <see cref="RequestAttachmentUploadCommand"/>: size and MIME policy.</summary>
public sealed class RequestAttachmentUploadCommandValidator : AbstractValidator<RequestAttachmentUploadCommand>
{
    /// <summary>Configures the validation rules for requesting an upload URL.</summary>
    public RequestAttachmentUploadCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty().WithMessage("Task id is required.");
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(255).WithMessage("File name must not exceed 255 characters.");
        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than zero.")
            .LessThanOrEqualTo(AttachmentPolicy.MaxSizeBytes)
            .WithMessage($"File size must not exceed {AttachmentPolicy.MaxSizeBytes} bytes.");
        RuleFor(x => x.MimeType)
            .NotEmpty().WithMessage("MIME type is required.")
            .Must(AttachmentPolicy.IsAllowedMime).WithMessage("That file type is not permitted.");
    }
}
