using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.CreateOrganisation;

/// <summary>
/// Validates <see cref="CreateOrganisationCommand"/>. Input-policy rules live here; the
/// <c>ValidationBehaviour</c> runs this before the handler and a failure surfaces as an
/// HTTP 422 problem-details response.
/// </summary>
public sealed class CreateOrganisationCommandValidator : AbstractValidator<CreateOrganisationCommand>
{
    /// <summary>Configures the validation rules for creating an organisation.</summary>
    public CreateOrganisationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organisation name is required.")
            .MinimumLength(2).WithMessage("Organisation name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Organisation name must not exceed 100 characters.");

        // Slug is optional; when supplied it must be a valid URL-safe slug.
        When(x => !string.IsNullOrWhiteSpace(x.Slug), () =>
            RuleFor(x => x.Slug!)
                .MaximumLength(100).WithMessage("Slug must not exceed 100 characters.")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Slug must be lowercase letters, digits, and single hyphens."));
    }
}
