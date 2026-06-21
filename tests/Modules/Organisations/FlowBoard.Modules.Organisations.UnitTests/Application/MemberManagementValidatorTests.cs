using FlowBoard.Modules.Organisations.Application.Commands.ChangeMemberRole;
using FlowBoard.Modules.Organisations.Application.Commands.RemoveMember;
using FlowBoard.Modules.Organisations.Application.Commands.TransferOwnership;
using FluentValidation.TestHelper;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class MemberManagementValidatorTests
{
    private readonly ChangeMemberRoleCommandValidator _changeRole = new();
    private readonly RemoveMemberCommandValidator _remove = new();
    private readonly TransferOwnershipCommandValidator _transfer = new();

    [Theory]
    [InlineData("Admin")]
    [InlineData("member")]
    [InlineData("GUEST")]
    public void ChangeRole_passes_for_an_assignable_role(string role)
    {
        _changeRole
            .TestValidate(new ChangeMemberRoleCommand(Guid.NewGuid(), Guid.NewGuid(), role))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Owner")]
    [InlineData("")]
    public void ChangeRole_fails_for_owner_or_empty_role(string role)
    {
        _changeRole
            .TestValidate(new ChangeMemberRoleCommand(Guid.NewGuid(), Guid.NewGuid(), role))
            .ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void ChangeRole_fails_for_an_empty_user_id()
    {
        _changeRole
            .TestValidate(new ChangeMemberRoleCommand(Guid.NewGuid(), Guid.Empty, "Member"))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Remove_fails_for_an_empty_user_id()
    {
        _remove
            .TestValidate(new RemoveMemberCommand(Guid.NewGuid(), Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Transfer_fails_for_an_empty_new_owner_id()
    {
        _transfer
            .TestValidate(new TransferOwnershipCommand(Guid.NewGuid(), Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.NewOwnerId);
    }
}
