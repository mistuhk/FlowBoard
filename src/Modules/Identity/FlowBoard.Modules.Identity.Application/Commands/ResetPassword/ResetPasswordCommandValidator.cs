using FluentValidation;

namespace FlowBoard.Modules.Identity.Application.Commands.ResetPassword;

/// <summary>
/// Validates <see cref="ResetPasswordCommand"/>. The new password must meet the same strength
/// policy as registration; the token must be present.
/// </summary>
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    /// <summary>Configures the validation rules for completing a password reset.</summary>
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
