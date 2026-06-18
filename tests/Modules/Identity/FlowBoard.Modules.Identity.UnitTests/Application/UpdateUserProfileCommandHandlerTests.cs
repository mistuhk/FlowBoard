using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Commands.UpdateUserProfile;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUserRepository> _users = new();

    private UpdateUserProfileCommandHandler CreateHandler() => new(_currentUser.Object, _users.Object);

    private static User SomeUser() =>
        User.Register(
            Email.Create("ada@example.com"),
            HashedPassword.FromHash("$argon2id$hash"),
            DisplayName.Create("Ada Lovelace"));

    [Fact]
    public async Task Handle_updates_the_display_name_and_returns_the_updated_profile()
    {
        var user = SomeUser();
        _currentUser.SetupGet(c => c.UserId).Returns(user.Id);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await CreateHandler().Handle(
            new UpdateUserProfileCommand("Augusta King"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayName.Should().Be("Augusta King");
        user.DisplayName.Value.Should().Be("Augusta King");
    }

    [Fact]
    public async Task Handle_fails_when_the_authenticated_user_no_longer_exists()
    {
        _currentUser.SetupGet(c => c.UserId).Returns(UserId.New());
        _users.Setup(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(
            new UpdateUserProfileCommand("Augusta King"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.UserNotFound);
    }
}
