using FlowBoard.Modules.Identity.Domain.Aggregates;

namespace FlowBoard.Modules.Identity.Application.Queries.GetUserProfile;

/// <summary>
/// The current user's profile. Returned by the profile query and the profile-update command.
/// </summary>
/// <param name="Id">The user's identifier.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="AvatarUrl">The user's avatar URL, or <c>null</c> if none is set.</param>
/// <param name="IsEmailVerified">Whether the user has verified their email address.</param>
/// <param name="CreatedAt">When the account was created (UTC).</param>
/// <param name="LastLoginAt">When the user last logged in (UTC), or <c>null</c> if never.</param>
public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt)
{
    /// <summary>Maps a <see cref="User"/> aggregate to its profile representation.</summary>
    /// <param name="user">The user to map.</param>
    public static UserProfileResponse From(User user) =>
        new(
            user.Id.Value,
            user.Email.Value,
            user.DisplayName.Value,
            user.AvatarUrl?.ToString(),
            user.IsEmailVerified,
            user.CreatedAt,
            user.LastLoginAt);
}
