using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.AcceptInvitation;

/// <summary>Validates <see cref="AcceptInvitationCommand"/>.</summary>
public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    /// <summary>Configures the validation rules for accepting an invitation.</summary>
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required.");
    }
}
