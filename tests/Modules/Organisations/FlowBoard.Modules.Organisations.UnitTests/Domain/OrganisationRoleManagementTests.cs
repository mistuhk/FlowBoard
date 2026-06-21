using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Events;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Organisations.UnitTests.Domain;

public sealed class OrganisationRoleManagementTests
{
    private static readonly UserId OwnerId = UserId.New();

    private static Organisation Create() =>
        Organisation.Create(
            OrganisationName.Create("Acme Industries"),
            OrganisationSlug.Create("acme-industries"),
            OwnerId);

    // Adds a member in the given role via the invite-and-accept flow, returning the new member's id.
    private static UserId AddMember(Organisation organisation, MemberRole role, string token)
    {
        organisation.InviteMember($"{token}@example.com", role, OwnerId, token);
        var userId = UserId.New();
        organisation.AcceptInvitation(token, userId);
        return userId;
    }

    private static int OwnerCount(Organisation organisation) =>
        organisation.Memberships.Count(m => m.Role == MemberRole.Owner);

    // -- ChangeMemberRole --

    [Fact]
    public void ChangeMemberRole_by_the_owner_updates_the_role_and_raises_an_event()
    {
        var organisation = Create();
        var memberId = AddMember(organisation, MemberRole.Guest, "guest-token");
        organisation.ClearDomainEvents();

        organisation.ChangeMemberRole(memberId, MemberRole.Member, OwnerId);

        organisation.Memberships.Single(m => m.UserId == memberId).Role.Should().Be(MemberRole.Member);
        var @event = organisation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MemberRoleChangedEvent>().Subject;
        @event.OldRole.Should().Be("Guest");
        @event.NewRole.Should().Be("Member");
    }

    [Fact]
    public void ChangeMemberRole_cannot_assign_owner()
    {
        var organisation = Create();
        var memberId = AddMember(organisation, MemberRole.Member, "member-token");

        var act = () => organisation.ChangeMemberRole(memberId, MemberRole.Owner, OwnerId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeMemberRole_by_a_non_member_is_forbidden()
    {
        var organisation = Create();
        var memberId = AddMember(organisation, MemberRole.Member, "member-token");

        var act = () => organisation.ChangeMemberRole(memberId, MemberRole.Guest, UserId.New());

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void ChangeMemberRole_cannot_promote_to_the_callers_own_level()
    {
        var organisation = Create();
        var adminId = AddMember(organisation, MemberRole.Admin, "admin-token");
        var memberId = AddMember(organisation, MemberRole.Member, "member-token");

        // An Admin cannot promote a Member to Admin (does not outrank the new role).
        var act = () => organisation.ChangeMemberRole(memberId, MemberRole.Admin, adminId);

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void ChangeMemberRole_to_the_same_role_raises_no_event()
    {
        var organisation = Create();
        var memberId = AddMember(organisation, MemberRole.Member, "member-token");
        organisation.ClearDomainEvents();

        organisation.ChangeMemberRole(memberId, MemberRole.Member, OwnerId);

        organisation.DomainEvents.Should().BeEmpty();
    }

    // -- RemoveMember --

    [Fact]
    public void RemoveMember_by_the_owner_removes_the_member_and_raises_an_event()
    {
        var organisation = Create();
        var memberId = AddMember(organisation, MemberRole.Member, "member-token");
        organisation.ClearDomainEvents();

        organisation.RemoveMember(memberId, OwnerId);

        organisation.Memberships.Should().NotContain(m => m.UserId == memberId);
        organisation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MemberRemovedEvent>()
            .Which.UserId.Should().Be(memberId);
    }

    [Fact]
    public void RemoveMember_cannot_remove_the_owner()
    {
        var organisation = Create();
        var adminId = AddMember(organisation, MemberRole.Admin, "admin-token");

        var act = () => organisation.RemoveMember(OwnerId, adminId);

        act.Should().Throw<DomainException>();
        organisation.Memberships.Should().Contain(m => m.UserId == OwnerId);
    }

    [Fact]
    public void RemoveMember_requires_the_remover_to_outrank_the_target()
    {
        var organisation = Create();
        var firstAdmin = AddMember(organisation, MemberRole.Admin, "admin-1");
        var secondAdmin = AddMember(organisation, MemberRole.Admin, "admin-2");

        var act = () => organisation.RemoveMember(secondAdmin, firstAdmin);

        act.Should().Throw<ForbiddenException>();
    }

    // -- TransferOwnership --

    [Fact]
    public void TransferOwnership_moves_the_owner_role_and_demotes_the_previous_owner()
    {
        var organisation = Create();
        var successorId = AddMember(organisation, MemberRole.Admin, "successor-token");
        organisation.ClearDomainEvents();

        organisation.TransferOwnership(successorId, OwnerId);

        organisation.OwnerId.Should().Be(successorId);
        organisation.Memberships.Single(m => m.UserId == successorId).Role.Should().Be(MemberRole.Owner);
        organisation.Memberships.Single(m => m.UserId == OwnerId).Role.Should().Be(MemberRole.Admin);
        OwnerCount(organisation).Should().Be(1);

        var @event = organisation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OwnershipTransferredEvent>().Subject;
        @event.FromUserId.Should().Be(OwnerId);
        @event.ToUserId.Should().Be(successorId);
    }

    [Fact]
    public void TransferOwnership_by_a_non_owner_is_forbidden()
    {
        var organisation = Create();
        var adminId = AddMember(organisation, MemberRole.Admin, "admin-token");

        var act = () => organisation.TransferOwnership(adminId, adminId);

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void TransferOwnership_to_a_non_member_is_rejected()
    {
        var organisation = Create();

        var act = () => organisation.TransferOwnership(UserId.New(), OwnerId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TransferOwnership_to_the_current_owner_is_rejected()
    {
        var organisation = Create();

        var act = () => organisation.TransferOwnership(OwnerId, OwnerId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_organisation_always_has_exactly_one_owner_through_a_transfer()
    {
        var organisation = Create();
        OwnerCount(organisation).Should().Be(1);

        var successorId = AddMember(organisation, MemberRole.Member, "successor-token");
        organisation.TransferOwnership(successorId, OwnerId);

        OwnerCount(organisation).Should().Be(1);
    }
}
