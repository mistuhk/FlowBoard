using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Identity.Domain.Events;

/// <summary>
/// Raised when a user successfully verifies their email address via <c>User.VerifyEmail</c>.
/// After this event is processed the user is permitted to log in.
/// </summary>
/// <param name="UserId">The identifier of the user who verified their email.</param>
public sealed record EmailVerifiedEvent(UserId UserId) : DomainEvent;
