using FluentValidation;

namespace FlowBoard.Modules.Identity.Application.Commands.RequestPasswordReset;

/// <summary>Validates <see cref="RequestPasswordResetCommand"/>: a well-formed email is required.</summary>
public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    /// <summary>Initialises the validator.</summary>
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);
    }
}
