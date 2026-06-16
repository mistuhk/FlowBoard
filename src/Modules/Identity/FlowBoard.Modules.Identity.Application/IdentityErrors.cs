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
}
