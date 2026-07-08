using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.DeclineInvitation;

/// <summary>Validates <see cref="DeclineInvitationCommand"/>.</summary>
public sealed class DeclineInvitationCommandValidator : AbstractValidator<DeclineInvitationCommand>
{
    /// <summary>Configures the validation rules for declining an invitation.</summary>
    public DeclineInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required.");
    }
}
