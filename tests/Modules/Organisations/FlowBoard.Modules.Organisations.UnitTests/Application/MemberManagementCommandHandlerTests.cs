using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Commands.ChangeMemberRole;
using FlowBoard.Modules.Organisations.Application.Commands.RemoveMember;
using FlowBoard.Modules.Organisations.Application.Commands.TransferOwnership;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class MemberManagementCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IOrganisationRepository> _organisations = new();
    private readonly UserId _ownerId = UserId.New();

    private Organisation NewOrganisationWithMember(MemberRole role, out UserId memberId)
    {
        var organisation = Organisation.Create(
            OrganisationName.Create("Acme Industries"),
            OrganisationSlug.Create("acme-industries"),
            _ownerId);
        organisation.InviteMember("member@example.com", role, _ownerId, "token");
        memberId = UserId.New();
        organisation.AcceptInvitation("token", memberId);

        _organisations
            .Setup(r => r.GetByIdAsync(It.IsAny<OrganisationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        return organisation;
    }

    private void Caller(UserId userId) => _currentUser.Setup(c => c.UserId).Returns(userId);

    // -- ChangeMemberRole --

    [Fact]
    public async Task ChangeRole_returns_not_found_when_the_organisation_is_missing()
    {
        _organisations
            .Setup(r => r.GetByIdAsync(It.IsAny<OrganisationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);
        Caller(_ownerId);
        var handler = new ChangeMemberRoleCommandHandler(_currentUser.Object, _organisations.Object);

        var result = await handler.Handle(
            new ChangeMemberRoleCommand(Guid.NewGuid(), Guid.NewGuid(), "Member"), CancellationToken.None);

        result.Error.Should().Be(OrganisationErrors.NotFound);
    }

    [Fact]
    public async Task ChangeRole_applies_the_new_role()
    {
        var organisation = NewOrganisationWithMember(MemberRole.Guest, out var memberId);
        Caller(_ownerId);
        var handler = new ChangeMemberRoleCommandHandler(_currentUser.Object, _organisations.Object);

        var result = await handler.Handle(
            new ChangeMemberRoleCommand(organisation.Id.Value, memberId.Value, "Member"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        organisation.Memberships.Single(m => m.UserId == memberId).Role.Should().Be(MemberRole.Member);
    }

    // -- RemoveMember --

    [Fact]
    public async Task Remove_removes_the_member()
    {
        var organisation = NewOrganisationWithMember(MemberRole.Member, out var memberId);
        Caller(_ownerId);
        var handler = new RemoveMemberCommandHandler(_currentUser.Object, _organisations.Object);

        var result = await handler.Handle(
            new RemoveMemberCommand(organisation.Id.Value, memberId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        organisation.Memberships.Should().NotContain(m => m.UserId == memberId);
    }

    [Fact]
    public async Task Remove_throws_when_targeting_the_owner()
    {
        var organisation = NewOrganisationWithMember(MemberRole.Admin, out var adminId);
        Caller(adminId);
        var handler = new RemoveMemberCommandHandler(_currentUser.Object, _organisations.Object);

        var act = () => handler.Handle(
            new RemoveMemberCommand(organisation.Id.Value, _ownerId.Value), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    // -- TransferOwnership --

    [Fact]
    public async Task Transfer_moves_ownership_to_the_member()
    {
        var organisation = NewOrganisationWithMember(MemberRole.Admin, out var successorId);
        Caller(_ownerId);
        var handler = new TransferOwnershipCommandHandler(_currentUser.Object, _organisations.Object);

        var result = await handler.Handle(
            new TransferOwnershipCommand(organisation.Id.Value, successorId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        organisation.OwnerId.Should().Be(successorId);
    }

    [Fact]
    public async Task Transfer_by_a_non_owner_is_forbidden()
    {
        var organisation = NewOrganisationWithMember(MemberRole.Admin, out var adminId);
        Caller(adminId);
        var handler = new TransferOwnershipCommandHandler(_currentUser.Object, _organisations.Object);

        var act = () => handler.Handle(
            new TransferOwnershipCommand(organisation.Id.Value, adminId.Value), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
