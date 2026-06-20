using FlowBoard.Modules.Organisations.Application.Commands.AcceptInvitation;
using FlowBoard.Modules.Organisations.Application.Commands.DeclineInvitation;
using FlowBoard.Modules.Organisations.Application.Commands.InviteMember;
using FluentValidation.TestHelper;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class InvitationValidatorTests
{
    private readonly InviteMemberCommandValidator _invite = new();
    private readonly AcceptInvitationCommandValidator _accept = new();
    private readonly DeclineInvitationCommandValidator _decline = new();

    [Theory]
    [InlineData("Admin")]
    [InlineData("member")]
    [InlineData("GUEST")]
    public void Invite_passes_for_an_invitable_role_in_any_case(string role)
    {
        var result = _invite.TestValidate(
            new InviteMemberCommand(Guid.NewGuid(), "invitee@example.com", role));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Owner")]
    [InlineData("superuser")]
    [InlineData("")]
    public void Invite_fails_for_a_non_invitable_or_empty_role(string role)
    {
        var result = _invite.TestValidate(
            new InviteMemberCommand(Guid.NewGuid(), "invitee@example.com", role));

        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Invite_fails_for_an_invalid_email()
    {
        var result = _invite.TestValidate(
            new InviteMemberCommand(Guid.NewGuid(), "not-an-email", "Member"));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Invite_fails_for_an_empty_organisation_id()
    {
        var result = _invite.TestValidate(
            new InviteMemberCommand(Guid.Empty, "invitee@example.com", "Member"));

        result.ShouldHaveValidationErrorFor(x => x.OrganisationId);
    }

    [Fact]
    public void Accept_fails_for_an_empty_token()
    {
        _accept.TestValidate(new AcceptInvitationCommand("")).ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Decline_fails_for_an_empty_token()
    {
        _decline.TestValidate(new DeclineInvitationCommand("")).ShouldHaveValidationErrorFor(x => x.Token);
    }
}
