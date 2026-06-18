using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application.Commands.ResetPassword;

/// <summary>
/// Completes a password reset: sets a new password from a valid reset token and revokes the
/// account's existing sessions.
/// </summary>
/// <param name="Token">The single-use reset token from the reset link.</param>
/// <param name="NewPassword">The new plaintext password (hashed by the handler).</param>
public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand<Result>;
