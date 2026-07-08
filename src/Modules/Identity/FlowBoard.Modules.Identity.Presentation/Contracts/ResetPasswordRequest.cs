namespace FlowBoard.Modules.Identity.Presentation.Contracts;

/// <summary>The request body for <c>POST /api/v1/auth/reset-password</c>.</summary>
/// <param name="Token">The single-use reset token from the reset link.</param>
/// <param name="NewPassword">The new password to set.</param>
public sealed record ResetPasswordRequest(string Token, string NewPassword);
