using FlowBoard.Modules.Identity.Application.Commands.ResetPassword;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public void A_command_with_a_token_and_a_strong_password_is_valid()
    {
        _validator.Validate(new ResetPasswordCommand("a-token", "Password1")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Password1")]      // missing token
    [InlineData("a-token", "")]         // missing password
    [InlineData("a-token", "short1A")]  // too short
    [InlineData("a-token", "password1")] // no uppercase
    [InlineData("a-token", "PASSWORD1")] // no lowercase
    [InlineData("a-token", "Password")]  // no digit
    public void A_command_with_a_missing_token_or_weak_password_is_invalid(string token, string password)
    {
        _validator.Validate(new ResetPasswordCommand(token, password)).IsValid.Should().BeFalse();
    }
}
