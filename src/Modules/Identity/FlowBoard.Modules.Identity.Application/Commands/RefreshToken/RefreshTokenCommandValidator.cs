using FluentValidation;

namespace FlowBoard.Modules.Identity.Application.Commands.RefreshToken;

/// <summary>Validates <see cref="RefreshTokenCommand"/>: the refresh token must be present.</summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>Initialises the validator.</summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty();
    }
}
