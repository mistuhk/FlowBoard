using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.RenameOrganisation;

/// <summary>
/// Validates <see cref="RenameOrganisationCommand"/>. Input-policy rules live here; the
/// <c>ValidationBehaviour</c> runs this before the handler and a failure surfaces as an
/// HTTP 422 problem-details response.
/// </summary>
public sealed class RenameOrganisationCommandValidator : AbstractValidator<RenameOrganisationCommand>
{
    /// <summary>Configures the validation rules for renaming an organisation.</summary>
    public RenameOrganisationCommandValidator()
    {
        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("Organisation id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organisation name is required.")
            .MinimumLength(2).WithMessage("Organisation name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Organisation name must not exceed 100 characters.");
    }
}
