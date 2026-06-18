using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Identity.Domain.Events;

/// <summary>
/// Raised when a user's password is changed via <c>User.ChangePassword</c> (for example by
/// completing a password reset). Other modules may react, for instance to notify the user that
/// their password was changed.
/// </summary>
/// <param name="UserId">The identifier of the user whose password changed.</param>
public sealed record PasswordChangedEvent(UserId UserId) : DomainEvent;
