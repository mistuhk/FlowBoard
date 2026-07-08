using FluentValidation;

namespace FlowBoard.Modules.Notifications.Application.Commands.CreateNotification;

/// <summary>Validates <see cref="CreateNotificationCommand"/>.</summary>
public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    /// <summary>Configures the validation rules for creating a notification.</summary>
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User id is required.");
        RuleFor(x => x.OrganisationId).NotEmpty().WithMessage("Organisation id is required.");
        RuleFor(x => x.Type).NotEmpty().WithMessage("Notification type is required.");
        RuleFor(x => x.Message).NotEmpty().WithMessage("Message is required.");
    }
}
