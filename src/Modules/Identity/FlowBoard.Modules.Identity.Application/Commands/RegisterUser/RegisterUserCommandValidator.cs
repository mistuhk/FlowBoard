using FluentValidation;

namespace FlowBoard.Modules.Identity.Application.Commands.RegisterUser;

/// <summary>
/// Validates <see cref="RegisterUserCommand"/>. Input-policy rules (format, length,
/// password strength) live here; the <c>ValidationBehaviour</c> runs this before the
/// handler and a failure surfaces as an HTTP 422 problem-details response.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>Configures the validation rules for registration.</summary>
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254).WithMessage("Email address must not exceed 254 characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
