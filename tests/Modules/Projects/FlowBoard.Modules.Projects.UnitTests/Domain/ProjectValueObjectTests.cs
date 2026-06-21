using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Projects.UnitTests.Domain;

public sealed class ProjectValueObjectTests
{
    [Fact]
    public void ProjectName_trims_and_accepts_up_to_150_characters()
    {
        ProjectName.Create("  Hello  ").Value.Should().Be("Hello");
        ProjectName.Create(new string('a', 150)).Value.Length.Should().Be(150);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ProjectName_rejects_empty_values(string value)
    {
        var act = () => ProjectName.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ProjectName_rejects_values_longer_than_150_characters()
    {
        var act = () => ProjectName.Create(new string('a', 151));

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("active")]
    [InlineData("Active")]
    [InlineData("ARCHIVED")]
    public void ProjectStatus_round_trips_from_persistence_case_insensitively(string stored)
    {
        var status = ProjectStatus.FromPersistence(stored);

        status.Name.ToLowerInvariant().Should().Be(stored.ToLowerInvariant());
    }

    [Fact]
    public void ProjectStatus_rejects_an_unknown_value()
    {
        var act = () => ProjectStatus.FromPersistence("paused");

        act.Should().Throw<DomainException>();
    }
}
