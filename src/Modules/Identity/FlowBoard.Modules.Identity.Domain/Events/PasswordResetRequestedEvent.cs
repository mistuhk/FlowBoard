using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Identity.Domain.Events;

/// <summary>
/// Raised when a user requests a password reset via <c>User.RequestPasswordReset</c>. A handler
/// sends the reset email from the outbox, so delivery stays asynchronous and the Application layer
/// keeps no email dependency. This records a request, not a state change.
/// </summary>
/// <param name="UserId">The identifier of the user who requested the reset.</param>
/// <param name="Email">The email address the reset link should be sent to.</param>
public sealed record PasswordResetRequestedEvent(UserId UserId, string Email) : DomainEvent;
