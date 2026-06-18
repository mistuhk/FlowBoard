using FluentValidation;

namespace FlowBoard.Modules.Identity.Application.Commands.UpdateUserProfile;

/// <summary>Validates <see cref="UpdateUserProfileCommand"/>: the display name policy from registration.</summary>
public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    /// <summary>Initialises the validator.</summary>
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(c => c.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");
    }
}
