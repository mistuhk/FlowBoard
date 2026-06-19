using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Organisations.UnitTests.Domain;

public sealed class MemberRoleTests
{
    [Fact]
    public void Roles_are_ordered_owner_above_admin_above_member_above_guest()
    {
        MemberRole.Owner.Rank.Should().BeGreaterThan(MemberRole.Admin.Rank);
        MemberRole.Admin.Rank.Should().BeGreaterThan(MemberRole.Member.Rank);
        MemberRole.Member.Rank.Should().BeGreaterThan(MemberRole.Guest.Rank);
    }

    [Fact]
    public void Outranks_is_true_only_for_a_strictly_higher_role()
    {
        MemberRole.Admin.Outranks(MemberRole.Member).Should().BeTrue();
        MemberRole.Admin.Outranks(MemberRole.Admin).Should().BeFalse();
        MemberRole.Member.Outranks(MemberRole.Admin).Should().BeFalse();
    }

    [Fact]
    public void IsAtLeast_includes_the_same_rank()
    {
        MemberRole.Admin.IsAtLeast(MemberRole.Admin).Should().BeTrue();
        MemberRole.Admin.IsAtLeast(MemberRole.Member).Should().BeTrue();
        MemberRole.Member.IsAtLeast(MemberRole.Admin).Should().BeFalse();
    }

    [Fact]
    public void FromName_round_trips_every_role()
    {
        foreach (var role in MemberRole.All)
            MemberRole.FromName(role.Name).Should().Be(role);
    }

    [Fact]
    public void FromName_throws_for_an_unknown_role()
    {
        var act = () => MemberRole.FromName("Superuser");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Roles_with_the_same_name_are_equal()
    {
        MemberRole.FromName("Owner").Should().Be(MemberRole.Owner);
    }
}
