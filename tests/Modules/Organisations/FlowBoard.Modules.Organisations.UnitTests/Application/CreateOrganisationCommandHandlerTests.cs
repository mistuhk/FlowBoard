using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Commands.CreateOrganisation;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Organisations.UnitTests.Application;

public sealed class CreateOrganisationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IOrganisationRepository> _organisations = new();
    private readonly UserId _callerId = UserId.New();

    private CreateOrganisationCommandHandler CreateHandler()
    {
        _currentUser.Setup(c => c.UserId).Returns(_callerId);
        return new CreateOrganisationCommandHandler(_currentUser.Object, _organisations.Object);
    }

    [Fact]
    public async Task Handle_creates_the_organisation_with_the_caller_as_owner()
    {
        _organisations
            .Setup(r => r.ExistsBySlugAsync(It.IsAny<OrganisationSlug>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateOrganisationCommand("Acme Industries", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Acme Industries");
        result.Value.OwnerId.Should().Be(_callerId.Value);
        _organisations.Verify(
            r => r.AddAsync(It.IsAny<Organisation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_derives_the_slug_from_the_name_when_none_is_supplied()
    {
        _organisations
            .Setup(r => r.ExistsBySlugAsync(It.IsAny<OrganisationSlug>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateOrganisationCommand("Acme Industries", null), CancellationToken.None);

        result.Value.Slug.Should().Be("acme-industries");
    }

    [Fact]
    public async Task Handle_uses_the_supplied_slug_when_provided()
    {
        _organisations
            .Setup(r => r.ExistsBySlugAsync(It.IsAny<OrganisationSlug>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateOrganisationCommand("Acme Industries", "acme-co"), CancellationToken.None);

        result.Value.Slug.Should().Be("acme-co");
    }

    [Fact]
    public async Task Handle_returns_a_conflict_when_the_slug_is_already_in_use()
    {
        _organisations
            .Setup(r => r.ExistsBySlugAsync(It.IsAny<OrganisationSlug>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = CreateHandler();

        var result = await handler.Handle(
            new CreateOrganisationCommand("Acme Industries", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganisationErrors.SlugAlreadyInUse);
        _organisations.Verify(
            r => r.AddAsync(It.IsAny<Organisation>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
