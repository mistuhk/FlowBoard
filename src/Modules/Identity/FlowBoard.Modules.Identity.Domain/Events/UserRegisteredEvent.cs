using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Identity.Domain.Events;

/// <summary>
/// Raised when a new user account is created via <c>User.Register</c>.
/// Consumed by the Notifications module to send a verification email.
/// </summary>
/// <param name="UserId">The identifier of the newly registered user.</param>
/// <param name="Email">The email address of the newly registered user.</param>
public sealed record UserRegisteredEvent(UserId UserId, string Email) : DomainEvent;
