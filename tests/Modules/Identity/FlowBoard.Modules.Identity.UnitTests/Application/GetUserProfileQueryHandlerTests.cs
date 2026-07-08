using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Queries.GetUserProfile;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class GetUserProfileQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUserRepository> _users = new();

    private GetUserProfileQueryHandler CreateHandler() => new(_currentUser.Object, _users.Object);

    private static User SomeUser() =>
        User.Register(
            Email.Create("ada@example.com"),
            HashedPassword.FromHash("$argon2id$hash"),
            DisplayName.Create("Ada Lovelace"));

    [Fact]
    public async Task Handle_returns_the_profile_of_the_authenticated_user()
    {
        var user = SomeUser();
        _currentUser.SetupGet(c => c.UserId).Returns(user.Id);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await CreateHandler().Handle(new GetUserProfileQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id.Value);
        result.Value.Email.Should().Be("ada@example.com");
        result.Value.DisplayName.Should().Be("Ada Lovelace");
    }

    [Fact]
    public async Task Handle_fails_when_the_authenticated_user_no_longer_exists()
    {
        _currentUser.SetupGet(c => c.UserId).Returns(UserId.New());
        _users.Setup(u => u.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new GetUserProfileQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.UserNotFound);
    }
}
