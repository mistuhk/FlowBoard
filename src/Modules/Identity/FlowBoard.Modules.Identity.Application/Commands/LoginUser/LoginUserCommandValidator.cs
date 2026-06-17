using FluentValidation;

namespace FlowBoard.Modules.Identity.Application.Commands.LoginUser;

/// <summary>
/// Validates <see cref="LoginUserCommand"/>. Only presence and basic shape are checked here:
/// whether the credentials are actually correct is decided by the handler, which returns the
/// same opaque failure for every wrong-credential case.
/// </summary>
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    /// <summary>Initialises the validator.</summary>
    public LoginUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(c => c.Password)
            .NotEmpty()
            .MaximumLength(128);
    }
}
