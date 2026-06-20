using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Commands.InviteMember;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class InviteMemberCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IOrganisationRepository> _organisations = new();
    private readonly UserId _ownerId = UserId.New();

    private static Organisation NewOrganisation(UserId ownerId) =>
        Organisation.Create(
            OrganisationName.Create("Acme Industries"),
            OrganisationSlug.Create("acme-industries"),
            ownerId);

    private InviteMemberCommandHandler CreateHandler(UserId callerId)
    {
        _currentUser.Setup(c => c.UserId).Returns(callerId);
        return new InviteMemberCommandHandler(_currentUser.Object, _organisations.Object);
    }

    [Fact]
    public async Task Handle_returns_not_found_when_the_organisation_does_not_exist()
    {
        _organisations
            .Setup(r => r.GetByIdAsync(It.IsAny<OrganisationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);
        var handler = CreateHandler(_ownerId);

        var result = await handler.Handle(
            new InviteMemberCommand(Guid.NewGuid(), "invitee@example.com", "Member"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganisationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_issues_an_invitation_and_returns_the_raw_token()
    {
        var organisation = NewOrganisation(_ownerId);
        _organisations
            .Setup(r => r.GetByIdAsync(It.IsAny<OrganisationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        var handler = CreateHandler(_ownerId);

        var result = await handler.Handle(
            new InviteMemberCommand(organisation.Id.Value, "Invitee@Example.com", "Member"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrWhiteSpace();
        result.Value.InvitedEmail.Should().Be("invitee@example.com");
        result.Value.Role.Should().Be("Member");
        organisation.Invitations.Should().ContainSingle();

        // The stored hash must match the hash of the returned raw token.
        organisation.Invitations.Single().TokenHash.Should().Be(InvitationTokens.Hash(result.Value.Token));
    }

    [Fact]
    public async Task Handle_propagates_forbidden_when_the_caller_may_not_invite()
    {
        var organisation = NewOrganisation(_ownerId);
        _organisations
            .Setup(r => r.GetByIdAsync(It.IsAny<OrganisationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        var handler = CreateHandler(UserId.New());  // not a member

        var act = () => handler.Handle(
            new InviteMemberCommand(organisation.Id.Value, "invitee@example.com", "Member"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
