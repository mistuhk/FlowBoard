using FluentValidation;

namespace FlowBoard.Modules.Notifications.Application.Commands.MarkAsRead;

/// <summary>Validates <see cref="MarkAsReadCommand"/>.</summary>
public sealed class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    /// <summary>Configures the validation rules for marking a notification read.</summary>
    public MarkAsReadCommandValidator() =>
        RuleFor(x => x.NotificationId).NotEmpty().WithMessage("Notification id is required.");
}
