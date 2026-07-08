using FlowBoard.Modules.Identity.Application.Commands.UpdateUserProfile;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class UpdateUserProfileCommandValidatorTests
{
    private readonly UpdateUserProfileCommandValidator _validator = new();

    [Fact]
    public void A_command_with_a_display_name_is_valid()
    {
        _validator.Validate(new UpdateUserProfileCommand("Ada Lovelace")).IsValid.Should().BeTrue();
    }

    [Fact]
    public void A_command_without_a_display_name_is_invalid()
    {
        _validator.Validate(new UpdateUserProfileCommand("")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void A_display_name_longer_than_100_characters_is_invalid()
    {
        _validator.Validate(new UpdateUserProfileCommand(new string('a', 101))).IsValid.Should().BeFalse();
    }
}
