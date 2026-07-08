using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application.Commands.VerifyEmail;

/// <summary>
/// Confirms a user's email address using the single-use token issued at registration.
/// </summary>
/// <param name="Token">The opaque verification token from the emailed link.</param>
public sealed record VerifyEmailCommand(string Token) : ICommand<Result>;
