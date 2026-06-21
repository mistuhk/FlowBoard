using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Entities;
using FlowBoard.Modules.Organisations.Domain.Events;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Organisations.UnitTests.Domain;

public sealed class OrganisationInvitationTests
{
    private static readonly UserId OwnerId = UserId.New();

    private static Organisation Create() =>
        Organisation.Create(
            OrganisationName.Create("Acme Industries"),
            OrganisationSlug.Create("acme-industries"),
            OwnerId);

    // Adds a member in the given role by running the full invite-and-accept flow.
    private static UserId AddMember(Organisation organisation, MemberRole role, string tokenHash)
    {
        organisation.InviteMember($"{tokenHash}@example.com", role, OwnerId, tokenHash);
        var userId = UserId.New();
        organisation.AcceptInvitation(tokenHash, userId);
        return userId;
    }

    [Fact]
    public void InviteMember_by_the_owner_adds_a_pending_invitation_and_raises_MemberInvitedEvent()
    {
        var organisation = Create();
        organisation.ClearDomainEvents();

        organisation.InviteMember("Invitee@Example.com ", MemberRole.Member, OwnerId, "hash-1");

        var invitation = organisation.Invitations.Should().ContainSingle().Subject;
        invitation.InvitedEmail.Should().Be("invitee@example.com");
        invitation.Role.Should().Be(MemberRole.Member);
        invitation.InvitedById.Should().Be(OwnerId);
        invitation.AcceptedAt.Should().BeNull();
        invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(Invitation.Lifetime), TimeSpan.FromMinutes(1));

        var @event = organisation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MemberInvitedEvent>().Subject;
        @event.InvitedEmail.Should().Be("invitee@example.com");
        @event.Role.Should().Be("Member");
        @event.InvitedById.Should().Be(OwnerId);
    }

    [Fact]
    public void InviteMember_is_allowed_for_an_admin()
    {
        var organisation = Create();
        var adminId = AddMember(organisation, MemberRole.Admin, "admin-token");

        var act = () => organisation.InviteMember("new@example.com", MemberRole.Member, adminId, "hash-2");

        act.Should().NotThrow();
    }

    [Fact]
    public void InviteMember_by_a_plain_member_is_forbidden()
    {
        var organisation = Create();
        var memberId = AddMember(organisation, MemberRole.Member, "member-token");

        var act = () => organisation.InviteMember("new@example.com", MemberRole.Member, memberId, "hash-2");

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void InviteMember_by_a_non_member_is_forbidden()
    {
        var organisation = Create();

        var act = () => organisation.InviteMember("new@example.com", MemberRole.Member, UserId.New(), "hash-1");

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void InviteMember_as_owner_is_rejected()
    {
        var organisation = Create();

        var act = () => organisation.InviteMember("new@example.com", MemberRole.Owner, OwnerId, "hash-1");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void InviteMember_rejects_a_duplicate_pending_invitation_for_the_same_email()
    {
        var organisation = Create();
        organisation.InviteMember("dup@example.com", MemberRole.Member, OwnerId, "hash-1");

        var act = () => organisation.InviteMember("dup@example.com", MemberRole.Guest, OwnerId, "hash-2");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AcceptInvitation_adds_the_member_in_the_invited_role_and_raises_MemberJoinedEvent()
    {
        var organisation = Create();
        organisation.InviteMember("invitee@example.com", MemberRole.Member, OwnerId, "hash-1");
        organisation.ClearDomainEvents();
        var inviteeId = UserId.New();

        organisation.AcceptInvitation("hash-1", inviteeId);

        var membership = organisation.Memberships.Should().ContainSingle(m => m.UserId == inviteeId).Subject;
        membership.Role.Should().Be(MemberRole.Member);
        membership.InvitedById.Should().Be(OwnerId);
        organisation.Invitations.Single().AcceptedAt.Should().NotBeNull();

        organisation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MemberJoinedEvent>()
            .Which.UserId.Should().Be(inviteeId);
    }

    [Fact]
    public void AcceptInvitation_with_an_unknown_token_throws()
    {
        var organisation = Create();

        var act = () => organisation.AcceptInvitation("does-not-exist", UserId.New());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AcceptInvitation_a_second_time_throws_because_it_is_no_longer_pending()
    {
        var organisation = Create();
        organisation.InviteMember("invitee@example.com", MemberRole.Member, OwnerId, "hash-1");
        organisation.AcceptInvitation("hash-1", UserId.New());

        var act = () => organisation.AcceptInvitation("hash-1", UserId.New());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AcceptInvitation_throws_when_the_user_is_already_a_member()
    {
        var organisation = Create();
        var existingMemberId = AddMember(organisation, MemberRole.Member, "member-token");
        organisation.InviteMember("again@example.com", MemberRole.Admin, OwnerId, "hash-2");

        var act = () => organisation.AcceptInvitation("hash-2", existingMemberId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void DeclineInvitation_removes_the_pending_invitation()
    {
        var organisation = Create();
        organisation.InviteMember("invitee@example.com", MemberRole.Member, OwnerId, "hash-1");

        organisation.DeclineInvitation("hash-1");

        organisation.Invitations.Should().BeEmpty();
    }

    [Fact]
    public void DeclineInvitation_with_an_unknown_token_throws()
    {
        var organisation = Create();

        var act = () => organisation.DeclineInvitation("does-not-exist");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_freshly_issued_invitation_is_pending_now_but_not_after_its_lifetime()
    {
        var organisation = Create();
        organisation.InviteMember("invitee@example.com", MemberRole.Member, OwnerId, "hash-1");
        var invitation = organisation.Invitations.Single();

        invitation.IsPending(DateTime.UtcNow).Should().BeTrue();
        invitation.IsPending(DateTime.UtcNow.Add(Invitation.Lifetime).AddMinutes(1)).Should().BeFalse();
    }
}
