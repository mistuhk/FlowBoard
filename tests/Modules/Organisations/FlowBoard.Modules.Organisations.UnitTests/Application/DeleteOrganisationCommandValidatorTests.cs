using FlowBoard.Modules.Organisations.Application.Commands.DeleteOrganisation;
using FluentValidation.TestHelper;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class DeleteOrganisationCommandValidatorTests
{
    private readonly DeleteOrganisationCommandValidator _validator = new();

    [Fact]
    public void Passes_for_a_valid_command()
    {
        var result = _validator.TestValidate(new DeleteOrganisationCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_when_the_organisation_id_is_empty()
    {
        var result = _validator.TestValidate(new DeleteOrganisationCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.OrganisationId);
    }
}
