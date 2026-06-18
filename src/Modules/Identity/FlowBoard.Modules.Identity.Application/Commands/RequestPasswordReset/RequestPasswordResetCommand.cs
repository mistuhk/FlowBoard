using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application.Commands.RequestPasswordReset;

/// <summary>
/// Starts the password-reset flow for an email address. Always succeeds from the caller's point of
/// view: whether or not the email belongs to an account is never revealed.
/// </summary>
/// <param name="Email">The email address to send a reset link to, if it belongs to an account.</param>
public sealed record RequestPasswordResetCommand(string Email) : ICommand<Result>;
