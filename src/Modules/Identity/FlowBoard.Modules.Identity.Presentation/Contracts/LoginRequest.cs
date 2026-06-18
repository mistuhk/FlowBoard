namespace FlowBoard.Modules.Identity.Presentation.Contracts;

/// <summary>The request body for <c>POST /api/v1/auth/login</c>.</summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Password">The account's password.</param>
public sealed record LoginRequest(string Email, string Password);
