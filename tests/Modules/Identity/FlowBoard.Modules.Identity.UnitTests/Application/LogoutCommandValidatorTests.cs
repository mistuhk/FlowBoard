using FlowBoard.Modules.Identity.Application.Commands.Logout;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _validator = new();

    [Fact]
    public void A_command_with_a_token_id_is_valid()
    {
        _validator.Validate(new LogoutCommand("the-jti", DateTime.UtcNow.AddMinutes(15), "refresh"))
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void A_command_without_a_token_id_is_invalid()
    {
        _validator.Validate(new LogoutCommand("", DateTime.UtcNow.AddMinutes(15), null))
            .IsValid.Should().BeFalse();
    }
}
