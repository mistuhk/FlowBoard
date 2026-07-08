using FlowBoard.Modules.Identity.Application.Commands.RefreshToken;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void A_command_with_a_token_is_valid()
    {
        _validator.Validate(new RefreshTokenCommand("a-refresh-token")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_command_without_a_token_is_invalid(string token)
    {
        _validator.Validate(new RefreshTokenCommand(token)).IsValid.Should().BeFalse();
    }
}
