using FlowBoard.Modules.Organisations.Application.Commands.RenameOrganisation;
using FluentValidation.TestHelper;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class RenameOrganisationCommandValidatorTests
{
    private readonly RenameOrganisationCommandValidator _validator = new();

    [Fact]
    public void Passes_for_a_valid_command()
    {
        var result = _validator.TestValidate(
            new RenameOrganisationCommand(Guid.NewGuid(), "Acme Global"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_when_the_organisation_id_is_empty()
    {
        var result = _validator.TestValidate(
            new RenameOrganisationCommand(Guid.Empty, "Acme Global"));

        result.ShouldHaveValidationErrorFor(x => x.OrganisationId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void Fails_when_the_name_is_too_short(string name)
    {
        var result = _validator.TestValidate(
            new RenameOrganisationCommand(Guid.NewGuid(), name));

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
