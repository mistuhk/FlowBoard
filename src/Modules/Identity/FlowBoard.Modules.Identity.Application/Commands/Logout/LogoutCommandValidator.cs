using FluentValidation;

namespace FlowBoard.Modules.Identity.Application.Commands.Logout;

/// <summary>Validates <see cref="LogoutCommand"/>: the token id must be present.</summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    /// <summary>Initialises the validator.</summary>
    public LogoutCommandValidator()
    {
        RuleFor(c => c.TokenId).NotEmpty();
    }
}
