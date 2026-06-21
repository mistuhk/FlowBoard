using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Organisations.UnitTests.Domain;

public sealed class OrganisationNameTests
{
    [Fact]
    public void Create_trims_surrounding_whitespace()
    {
        var name = OrganisationName.Create("  Acme  ");

        name.Value.Should().Be("Acme");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData(" A ")]
    public void Create_rejects_names_shorter_than_two_characters(string value)
    {
        var act = () => OrganisationName.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_rejects_names_longer_than_one_hundred_characters()
    {
        var act = () => OrganisationName.Create(new string('a', 101));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Two_names_with_the_same_value_are_equal()
    {
        OrganisationName.Create("Acme").Should().Be(OrganisationName.Create("Acme"));
    }
}
