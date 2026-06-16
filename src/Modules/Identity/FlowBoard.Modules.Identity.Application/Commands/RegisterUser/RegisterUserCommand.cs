using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application.Commands.RegisterUser;

/// <summary>
/// Registers a new, unverified user account.
/// </summary>
/// <param name="Email">The prospective user's email address. Must be unique.</param>
/// <param name="Password">The plaintext password. Hashed with Argon2id before storage; never persisted in the clear.</param>
/// <param name="DisplayName">The user's chosen display name.</param>
public sealed record RegisterUserCommand(string Email, string Password, string DisplayName)
    : ICommand<Result<RegisterUserResponse>>;
