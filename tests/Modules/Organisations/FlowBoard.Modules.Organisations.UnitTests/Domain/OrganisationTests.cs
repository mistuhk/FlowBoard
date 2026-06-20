using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Events;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Organisations.UnitTests.Domain;

public sealed class OrganisationTests
{
    private static readonly UserId OwnerId = UserId.New();

    private static Organisation Create(UserId? ownerId = null) =>
        Organisation.Create(
            OrganisationName.Create("Acme Industries"),
            OrganisationSlug.Create("acme-industries"),
            ownerId ?? OwnerId);

    [Fact]
    public void Create_sets_the_supplied_name_slug_and_owner()
    {
        var organisation = Create();

        organisation.Name.Value.Should().Be("Acme Industries");
        organisation.Slug.Value.Should().Be("acme-industries");
        organisation.OwnerId.Should().Be(OwnerId);
        organisation.Id.Value.Should().NotBe(Guid.Empty);
        organisation.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_sets_CreatedAt_because_the_object_initialiser_bypasses_the_base_constructor()
    {
        var organisation = Create();

        organisation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_makes_the_creator_the_sole_owner()
    {
        var organisation = Create();

        var membership = organisation.Memberships.Should().ContainSingle().Subject;
        membership.UserId.Should().Be(OwnerId);
        membership.Role.Should().Be(MemberRole.Owner);
        membership.InvitedById.Should().BeNull();
    }

    [Fact]
    public void Create_results_in_exactly_one_owner_membership()
    {
        var organisation = Create();

        organisation.Memberships.Count(m => m.Role == MemberRole.Owner).Should().Be(1);
    }

    [Fact]
    public void Create_raises_a_single_OrganisationCreatedEvent_carrying_the_id_and_owner()
    {
        var organisation = Create();

        var @event = organisation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrganisationCreatedEvent>().Subject;
        @event.OrganisationId.Should().Be(organisation.Id);
        @event.OwnerId.Should().Be(OwnerId);
    }

    [Fact]
    public void Rename_by_the_owner_changes_the_name_and_raises_no_event()
    {
        var organisation = Create();
        organisation.ClearDomainEvents();

        organisation.Rename(OrganisationName.Create("Acme Global"), OwnerId);

        organisation.Name.Value.Should().Be("Acme Global");
        organisation.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rename_by_a_non_owner_throws_and_leaves_the_name_unchanged()
    {
        var organisation = Create();
        var stranger = UserId.New();

        var act = () => organisation.Rename(OrganisationName.Create("Acme Global"), stranger);

        act.Should().Throw<ForbiddenException>();
        organisation.Name.Value.Should().Be("Acme Industries");
    }

    [Fact]
    public void Delete_by_the_owner_soft_deletes_and_raises_OrganisationDeletedEvent()
    {
        var organisation = Create();
        organisation.ClearDomainEvents();

        organisation.Delete(OwnerId);

        organisation.DeletedAt.Should().NotBeNull();
        organisation.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        organisation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrganisationDeletedEvent>()
            .Which.OrganisationId.Should().Be(organisation.Id);
    }

    [Fact]
    public void Delete_by_a_non_owner_throws_and_leaves_the_organisation_active()
    {
        var organisation = Create();
        var stranger = UserId.New();

        var act = () => organisation.Delete(stranger);

        act.Should().Throw<ForbiddenException>();
        organisation.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Delete_is_idempotent_and_raises_no_further_event_when_already_deleted()
    {
        var organisation = Create();
        organisation.Delete(OwnerId);
        var firstDeletedAt = organisation.DeletedAt;
        organisation.ClearDomainEvents();

        organisation.Delete(OwnerId);

        organisation.DeletedAt.Should().Be(firstDeletedAt);
        organisation.DomainEvents.Should().BeEmpty();
    }
}
