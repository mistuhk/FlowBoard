namespace FlowBoard.Modules.Identity.Presentation.Contracts;

/// <summary>The request body for <c>POST /api/v1/auth/forgot-password</c>.</summary>
/// <param name="Email">The email address to send a reset link to, if it belongs to an account.</param>
public sealed record ForgotPasswordRequest(string Email);
