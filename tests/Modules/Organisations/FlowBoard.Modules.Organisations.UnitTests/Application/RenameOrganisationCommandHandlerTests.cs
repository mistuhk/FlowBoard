using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Commands.RenameOrganisation;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class RenameOrganisationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IOrganisationRepository> _organisations = new();
    private readonly UserId _ownerId = UserId.New();

    private static Organisation NewOrganisation(UserId ownerId) =>
        Organisation.Create(
            OrganisationName.Create("Acme Industries"),
            OrganisationSlug.Create("acme-industries"),
            ownerId);

    private RenameOrganisationCommandHandler CreateHandler(UserId callerId)
    {
        _currentUser.Setup(c => c.UserId).Returns(callerId);
        return new RenameOrganisationCommandHandler(_currentUser.Object, _organisations.Object);
    }

    [Fact]
    public async Task Handle_returns_not_found_when_the_organisation_does_not_exist()
    {
        _organisations
            .Setup(r => r.GetByIdAsync(It.IsAny<OrganisationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organisation?)null);
        var handler = CreateHandler(_ownerId);

        var result = await handler.Handle(
            new RenameOrganisationCommand(Guid.NewGuid(), "Acme Global"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganisationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_renames_when_the_caller_is_the_owner()
    {
        var organisation = NewOrganisation(_ownerId);
        _organisations
            .Setup(r => r.GetByIdAsync(It.IsAny<OrganisationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        var handler = CreateHandler(_ownerId);

        var result = await handler.Handle(
            new RenameOrganisationCommand(organisation.Id.Value, "Acme Global"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Acme Global");
        organisation.Name.Value.Should().Be("Acme Global");
    }

    [Fact]
    public async Task Handle_throws_forbidden_when_the_caller_is_not_the_owner()
    {
        var organisation = NewOrganisation(_ownerId);
        _organisations
            .Setup(r => r.GetByIdAsync(It.IsAny<OrganisationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        var handler = CreateHandler(UserId.New());

        var act = () => handler.Handle(
            new RenameOrganisationCommand(organisation.Id.Value, "Acme Global"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
