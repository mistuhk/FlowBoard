using FlowBoard.Modules.Identity.Application.Commands.RegisterUser;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void A_well_formed_command_is_valid()
    {
        var result = _validator.Validate(new RegisterUserCommand("ada@example.com", "Password1", "Ada"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Password1", "Ada")]               // missing email
    [InlineData("not-an-email", "Password1", "Ada")]   // malformed email
    [InlineData("ada@example.com", "short", "Ada")]    // password too short
    [InlineData("ada@example.com", "password1", "Ada")] // no uppercase
    [InlineData("ada@example.com", "PASSWORD1", "Ada")] // no lowercase
    [InlineData("ada@example.com", "Password", "Ada")]  // no digit
    [InlineData("ada@example.com", "Password1", "")]    // missing display name
    public void An_invalid_command_fails_validation(string email, string password, string displayName)
    {
        var result = _validator.Validate(new RegisterUserCommand(email, password, displayName));

        result.IsValid.Should().BeFalse();
    }
}
