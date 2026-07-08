namespace FlowBoard.Modules.Identity.Application.Commands.RegisterUser;

/// <summary>
/// The result of a successful registration, returned to the API caller.
/// Carries no sensitive data, never the password hash.
/// </summary>
/// <param name="Id">The identifier of the newly created user.</param>
/// <param name="Email">The normalised email address the account was registered with.</param>
/// <param name="DisplayName">The user's display name.</param>
public sealed record RegisterUserResponse(Guid Id, string Email, string DisplayName);
