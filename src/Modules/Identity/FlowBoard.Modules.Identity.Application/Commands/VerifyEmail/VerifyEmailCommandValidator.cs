using FluentValidation;

namespace FlowBoard.Modules.Identity.Application.Commands.VerifyEmail;

/// <summary>
/// Validates <see cref="VerifyEmailCommand"/>. A missing token is a malformed request and
/// surfaces as HTTP 422 before the handler runs.
/// </summary>
public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    /// <summary>Configures the validation rules for email verification.</summary>
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("A verification token is required.");
    }
}
