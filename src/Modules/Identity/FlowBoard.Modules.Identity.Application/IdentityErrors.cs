using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application;

/// <summary>
/// Well-known <see cref="Error"/> values returned by Identity command and query handlers.
/// Centralised so error codes stay stable and discoverable across the module.
/// </summary>
public static class IdentityErrors
{
    /// <summary>Returned when registration is attempted with an email that already exists.</summary>
    public static readonly Error EmailAlreadyInUse = new(
        "Identity.EmailAlreadyInUse",
        "A user with this email address already exists.");

    /// <summary>Returned when an email-verification token is unknown, expired, or already used.</summary>
    public static readonly Error InvalidVerificationToken = new(
        "Identity.InvalidVerificationToken",
        "The verification token is invalid or has expired.");

    /// <summary>
    /// Returned when login fails because the email is unknown or the password is wrong.
    /// Deliberately identical for both cases so the response cannot be used to discover
    /// which email addresses are registered.
    /// </summary>
    public static readonly Error InvalidCredentials = new(
        "Identity.InvalidCredentials",
        "The email address or password is incorrect.");

    /// <summary>Returned when login is attempted before the account's email has been verified.</summary>
    public static readonly Error EmailNotVerified = new(
        "Identity.EmailNotVerified",
        "The email address for this account has not been verified.");
}
