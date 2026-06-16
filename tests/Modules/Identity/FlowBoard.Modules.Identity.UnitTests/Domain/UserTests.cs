using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Events;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Identity.UnitTests.Domain;

public sealed class UserTests
{
    private static User Register() =>
        User.Register(
            Email.Create("Ada.Lovelace@Example.com"),
            HashedPassword.FromHash("$argon2id$v=19$m=65536,t=3,p=1$abc$def"),
            DisplayName.Create("Ada Lovelace"));

    [Fact]
    public void Register_creates_an_unverified_user_with_the_given_details()
    {
        var user = Register();

        user.Email.Value.Should().Be("ada.lovelace@example.com");
        user.DisplayName.Value.Should().Be("Ada Lovelace");
        user.IsEmailVerified.Should().BeFalse();
        user.LastLoginAt.Should().BeNull();
        user.DeletedAt.Should().BeNull();
        user.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Register_sets_CreatedAt_because_the_object_initialiser_bypasses_the_base_constructor()
    {
        var user = Register();

        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Register_raises_a_single_UserRegisteredEvent_carrying_the_id_and_email()
    {
        var user = Register();

        var @event = user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredEvent>().Subject;
        @event.UserId.Should().Be(user.Id);
        @event.Email.Should().Be("ada.lovelace@example.com");
    }

    [Fact]
    public void VerifyEmail_marks_the_account_verified_and_raises_EmailVerifiedEvent()
    {
        var user = Register();
        user.ClearDomainEvents();

        user.VerifyEmail();

        user.IsEmailVerified.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EmailVerifiedEvent>();
    }
}
