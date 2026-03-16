namespace FlowBoard.Domain.Shared.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated.
/// These represent expected, recoverable errors.
/// The global exception middleware maps them to HTTP 422 Unprocessable Entity responses.
/// </summary>
/// <param name="message">A human-readable description of the violated invariant.</param>
public class DomainException(string message) : Exception(message);

/// <summary>
/// Thrown when a requested resource cannot be found.
/// The global exception middleware maps this to an HTTP 404 Not Found response.
/// </summary>
/// <param name="resourceName">The name of the resource type (e.g. <c>"Task"</c>).</param>
/// <param name="key">The key or identifier that was not found.</param>
public class NotFoundException(string resourceName, object key)
    : Exception($"{resourceName} with key '{key}' was not found.");

/// <summary>
/// Thrown when the current user does not have permission to perform an action.
/// The global exception middleware maps this to an HTTP 403 Forbidden response.
/// </summary>
/// <param name="message">A description of the forbidden action.</param>
public class ForbiddenException(string message = "You do not have permission to perform this action.")
    : Exception(message);

/// <summary>
/// Thrown when an operation conflicts with the current state (e.g. duplicate email on registration).
/// The global exception middleware maps this to an HTTP 409 Conflict response.
/// </summary>
/// <param name="message">A description of the conflict.</param>
public class ConflictException(string message) : Exception(message);

/// <summary>
/// Thrown when a task status transition is not permitted by the domain state machine.
/// Extends <see cref="DomainException"/> and maps to HTTP 422.
/// </summary>
/// <param name="from">The current status.</param>
/// <param name="to">The requested target status.</param>
public class InvalidStatusTransitionException(string from, string to)
    : DomainException($"Cannot transition status from '{from}' to '{to}'.");
