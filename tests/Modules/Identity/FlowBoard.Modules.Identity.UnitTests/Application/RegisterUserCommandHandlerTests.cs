using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Application;
using FlowBoard.Modules.Identity.Application.Commands.RegisterUser;
using FlowBoard.Modules.Identity.Domain.Abstractions;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Identity.UnitTests.Application;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ICacheService> _cache = new();

    private RegisterUserCommandHandler CreateHandler() =>
        new(_users.Object, _passwordHasher.Object, _cache.Object);

    private static RegisterUserCommand ValidCommand() =>
        new("ada@example.com", "Password1", "Ada Lovelace");

    public RegisterUserCommandHandlerTests() =>
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(HashedPassword.FromHash("$argon2id$hash"));

    [Fact]
    public async Task Handle_returns_failure_and_adds_nothing_when_the_email_is_already_in_use()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityErrors.EmailAlreadyInUse);
        _users.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _cache.Verify(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_persists_the_user_and_returns_the_created_response_on_success()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        User? added = null;
        _users.Setup(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => added = u)
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added.Should().NotBeNull();
        result.Value.Id.Should().Be(added!.Id.Value);
        result.Value.Email.Should().Be("ada@example.com");
        result.Value.DisplayName.Should().Be("Ada Lovelace");
    }

    [Fact]
    public async Task Handle_stores_token_and_pending_verification_entries_with_a_24_hour_ttl()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        _cache.Verify(c => c.SetAsync(
                It.Is<string>(k => k.StartsWith("email_verification:token:")),
                It.IsAny<string>(),
                EmailVerificationTokens.Ttl,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _cache.Verify(c => c.SetAsync(
                It.Is<string>(k => k.StartsWith("email_verification:pending:")),
                It.IsAny<string>(),
                EmailVerificationTokens.Ttl,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
