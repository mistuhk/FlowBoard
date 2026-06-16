namespace FlowBoard.Modules.Identity.Presentation.Contracts;

/// <summary>
/// Request body for <c>POST /api/v1/auth/register</c>.
/// </summary>
/// <param name="Email">The email address to register. Must be unique.</param>
/// <param name="Password">The plaintext password; hashed server-side and never stored in the clear.</param>
/// <param name="DisplayName">The user's chosen display name.</param>
public sealed record RegisterUserRequest(string Email, string Password, string DisplayName);
