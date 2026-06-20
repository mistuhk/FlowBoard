using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Commands.AcceptInvitation;
using FlowBoard.Modules.Organisations.Application.Commands.DeclineInvitation;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class AcceptDeclineInvitationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IOrganisationRepository> _organisations = new();
    private readonly UserId _ownerId = UserId.New();
    private const string RawToken = "raw-invitation-token";

    // An organisation with one pending invitation for the given raw token.
    private Organisation OrganisationWithPendingInvitation()
    {
        var organisation = Organisation.Create(
            OrganisationName.Create("Acme Industries"),
            OrganisationSlug.Create("acme-industries"),
            _ownerId);
        organisation.InviteMember("invitee@example.com", MemberRole.Member, _ownerId, InvitationTokens.Hash(RawToken));
        return organisation;
    }

    // -- Accept --

    [Fact]
    public async Task Accept_returns_invalid_when_the_token_matches_no_invitation()
    {
        _organisations
            .Setup(r => r.GetByInvitationTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);
        _currentUser.Setup(c => c.UserId).Returns(UserId.New());
        var handler = new AcceptInvitationCommandHandler(_currentUser.Object, _organisations.Object);

        var result = await handler.Handle(new AcceptInvitationCommand("unknown"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganisationErrors.InvalidInvitation);
    }

    [Fact]
    public async Task Accept_joins_the_user_and_returns_the_organisation()
    {
        var organisation = OrganisationWithPendingInvitation();
        var inviteeId = UserId.New();
        _organisations
            .Setup(r => r.GetByInvitationTokenHashAsync(InvitationTokens.Hash(RawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _currentUser.Setup(c => c.UserId).Returns(inviteeId);
        var handler = new AcceptInvitationCommandHandler(_currentUser.Object, _organisations.Object);

        var result = await handler.Handle(new AcceptInvitationCommand(RawToken), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(organisation.Id.Value);
        organisation.Memberships.Should().Contain(m => m.UserId == inviteeId);
    }

    // -- Decline --

    [Fact]
    public async Task Decline_returns_invalid_when_the_token_matches_no_invitation()
    {
        _organisations
            .Setup(r => r.GetByInvitationTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);
        var handler = new DeclineInvitationCommandHandler(_organisations.Object);

        var result = await handler.Handle(new DeclineInvitationCommand("unknown"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganisationErrors.InvalidInvitation);
    }

    [Fact]
    public async Task Decline_removes_the_pending_invitation()
    {
        var organisation = OrganisationWithPendingInvitation();
        _organisations
            .Setup(r => r.GetByInvitationTokenHashAsync(InvitationTokens.Hash(RawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        var handler = new DeclineInvitationCommandHandler(_organisations.Object);

        var result = await handler.Handle(new DeclineInvitationCommand(RawToken), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        organisation.Invitations.Should().BeEmpty();
    }
}
