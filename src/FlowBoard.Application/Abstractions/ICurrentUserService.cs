using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Provides the authenticated user's identity for the current HTTP request.
/// Implemented in the API layer using <c>IHttpContextAccessor</c> and JWT claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The strongly-typed identifier of the authenticated user.</summary>
    UserId UserId { get; }

    /// <summary>The email address of the authenticated user.</summary>
    string Email { get; }

    /// <summary><c>true</c> if the current request is authenticated.</summary>
    bool IsAuthenticated { get; }
}
