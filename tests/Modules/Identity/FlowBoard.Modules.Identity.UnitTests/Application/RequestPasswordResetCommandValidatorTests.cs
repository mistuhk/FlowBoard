using FlowBoard.Modules.Identity.Application.Commands.RequestPasswordReset;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class RequestPasswordResetCommandValidatorTests
{
    private readonly RequestPasswordResetCommandValidator _validator = new();

    [Fact]
    public void A_command_with_a_valid_email_is_valid()
    {
        _validator.Validate(new RequestPasswordResetCommand("ada@example.com")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void A_command_with_a_missing_or_malformed_email_is_invalid(string email)
    {
        _validator.Validate(new RequestPasswordResetCommand(email)).IsValid.Should().BeFalse();
    }
}
