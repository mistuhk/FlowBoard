using FlowBoard.Modules.Identity.Application.Commands.VerifyEmail;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class VerifyEmailCommandValidatorTests
{
    private readonly VerifyEmailCommandValidator _validator = new();

    [Fact]
    public void A_command_with_a_token_is_valid()
    {
        _validator.Validate(new VerifyEmailCommand("some-token")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_command_without_a_token_is_invalid(string token)
    {
        _validator.Validate(new VerifyEmailCommand(token)).IsValid.Should().BeFalse();
    }
}
