using FlowBoard.Modules.Organisations.Application.Commands.CreateOrganisation;
using FluentValidation.TestHelper;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class CreateOrganisationCommandValidatorTests
{
    private readonly CreateOrganisationCommandValidator _validator = new();

    [Fact]
    public void Passes_for_a_valid_command_without_a_slug()
    {
        var result = _validator.TestValidate(new CreateOrganisationCommand("Acme Industries", null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void Fails_when_the_name_is_too_short(string name)
    {
        var result = _validator.TestValidate(new CreateOrganisationCommand(name, null));

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("Acme Co")]      // space, not slug-safe
    [InlineData("ACME")]         // uppercase
    [InlineData("acme--co")]     // consecutive hyphens
    public void Fails_when_the_supplied_slug_is_invalid(string slug)
    {
        var result = _validator.TestValidate(new CreateOrganisationCommand("Acme Industries", slug));

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Passes_for_a_valid_supplied_slug()
    {
        var result = _validator.TestValidate(new CreateOrganisationCommand("Acme Industries", "acme-co"));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
