using FluentValidation;

namespace FlowBoard.Modules.ActivityLog.Application.Commands.LogActivity;

/// <summary>Validates <see cref="LogActivityCommand"/>.</summary>
public sealed class LogActivityCommandValidator : AbstractValidator<LogActivityCommand>
{
    /// <summary>Configures the validation rules for appending an activity entry.</summary>
    public LogActivityCommandValidator()
    {
        RuleFor(x => x.OrganisationId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.EventType).NotEmpty().MaximumLength(100);
    }
}
