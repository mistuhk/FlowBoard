using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.TransferOwnership;

/// <summary>Validates <see cref="TransferOwnershipCommand"/>.</summary>
public sealed class TransferOwnershipCommandValidator : AbstractValidator<TransferOwnershipCommand>
{
    /// <summary>Configures the validation rules for transferring ownership.</summary>
    public TransferOwnershipCommandValidator()
    {
        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("Organisation id is required.");

        RuleFor(x => x.NewOwnerId)
            .NotEmpty().WithMessage("New owner id is required.");
    }
}
