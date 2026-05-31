using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Domain.Events;
using FlowBoard.Modules.Identity.Domain.ValueObjects;

namespace FlowBoard.Modules.Identity.Domain.Aggregates;

/// <summary>
/// Aggregate root for the Identity bounded context.
/// Encapsulates all behaviour related to user account lifecycle:
/// registration, email verification, password management, and profile updates.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private User() { }

    /// <summary>The user's verified email address. Unique across all users.</summary>
    public Email Email { get; private set; } = null!;

    /// <summary>The user's hashed password. Never exposed outside the aggregate.</summary>
    public HashedPassword Password { get; private set; } = null!;

    /// <summary>The user's chosen display name shown across the platform.</summary>
    public DisplayName DisplayName { get; private set; } = null!;

    /// <summary>Optional URL to the user's avatar image.</summary>
    public Uri? AvatarUrl { get; private set; }

    /// <summary>
    /// Whether the user has confirmed their email address.
    /// Users cannot log in until this is <c>true</c>.
    /// </summary>
    public bool IsEmailVerified { get; private set; }

    /// <summary>UTC timestamp of the most recent change to this aggregate.</summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>UTC timestamp of the user's most recent successful login. <c>null</c> if never logged in.</summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>UTC timestamp at which this account was soft-deleted. <c>null</c> if active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Creates a new, unverified user.
    /// The password must already be hashed, hashing is performed by IPasswordHasher in the Application layer.
    /// <see cref="UserRegisteredEvent"/>.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The plaintext password (hashed internally by <see cref="HashedPassword"/>).</param>
    /// <param name="displayName">The user's chosen display name.</param>
    /// <returns>A new <see cref="User"/> instance with <see cref="IsEmailVerified"/> set to <c>false</c>.</returns>
    public static User Register(Email email, HashedPassword password, DisplayName displayName)
    {
        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = UserId.New(),
            Email = email,
            Password = password,
            DisplayName = displayName,
            IsEmailVerified = false,
            CreatedAt = now,     // object initialiser bypasses the base ctor, so set it explicitly
            UpdatedAt = now,
        };

        user.Raise(new UserRegisteredEvent(user.Id, email.Value));
        return user;
    }

    /// <summary>
    /// Marks the user's email as verified and raises <see cref="EmailVerifiedEvent"/>.
    /// </summary>
    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;

        Raise(new EmailVerifiedEvent(Id));
    }

    /// <summary>Records a successful login by updating <see cref="LastLoginAt"/>.</summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
