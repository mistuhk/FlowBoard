using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.RemoveMember;

/// <summary>Validates <see cref="RemoveMemberCommand"/>.</summary>
public sealed class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
{
    /// <summary>Configures the validation rules for removing a member.</summary>
    public RemoveMemberCommandValidator()
    {
        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("Organisation id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");
    }
}
