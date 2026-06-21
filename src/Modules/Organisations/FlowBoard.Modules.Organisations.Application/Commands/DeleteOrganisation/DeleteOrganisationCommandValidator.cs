using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.DeleteOrganisation;

/// <summary>
/// Validates <see cref="DeleteOrganisationCommand"/>. The <c>ValidationBehaviour</c> runs this
/// before the handler and a failure surfaces as an HTTP 422 problem-details response.
/// </summary>
public sealed class DeleteOrganisationCommandValidator : AbstractValidator<DeleteOrganisationCommand>
{
    /// <summary>Configures the validation rules for deleting an organisation.</summary>
    public DeleteOrganisationCommandValidator()
    {
        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("Organisation id is required.");
    }
}
