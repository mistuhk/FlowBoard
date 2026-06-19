using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Organisations.UnitTests.Domain;

public sealed class OrganisationSlugTests
{
    [Theory]
    [InlineData("acme")]
    [InlineData("acme-industries")]
    [InlineData("acme-2")]
    [InlineData("a1-b2-c3")]
    public void Create_accepts_valid_slugs(string value)
    {
        OrganisationSlug.Create(value).Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Acme")]            // uppercase
    [InlineData("acme industries")] // space
    [InlineData("-acme")]           // leading hyphen
    [InlineData("acme-")]           // trailing hyphen
    [InlineData("acme--industries")] // consecutive hyphens
    [InlineData("acme_industries")]  // underscore
    public void Create_rejects_invalid_slugs(string value)
    {
        var act = () => OrganisationSlug.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("Acme Industries", "acme-industries")]
    [InlineData("  Hello,  World!  ", "hello-world")]
    [InlineData("Café del Mar", "caf-del-mar")]
    [InlineData("FlowBoard 2.0", "flowboard-2-0")]
    public void FromName_derives_a_valid_slug(string name, string expected)
    {
        var slug = OrganisationSlug.FromName(OrganisationName.Create(name));

        slug.Value.Should().Be(expected);
    }

    [Fact]
    public void FromName_throws_when_the_name_has_no_alphanumeric_characters()
    {
        var act = () => OrganisationSlug.FromName(OrganisationName.Create("!!!"));

        act.Should().Throw<DomainException>();
    }
}
