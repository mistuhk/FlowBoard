using FlowBoard.Modules.Identity.Application.Commands.LoginUser;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void A_command_with_an_email_and_password_is_valid()
    {
        _validator.Validate(new LoginUserCommand("ada@example.com", "Password1"))
            .IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Password1")]
    [InlineData("not-an-email", "Password1")]
    [InlineData("ada@example.com", "")]
    public void A_command_missing_or_malformed_fields_is_invalid(string email, string password)
    {
        _validator.Validate(new LoginUserCommand(email, password))
            .IsValid.Should().BeFalse();
    }
}
