using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application.Commands.LoginUser;

/// <summary>
/// Authenticates a user by email and password and, on success, issues an access token and a
/// refresh token.
/// </summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Password">The plaintext password to verify.</param>
public sealed record LoginUserCommand(string Email, string Password)
    : ICommand<Result<LoginUserResponse>>;
