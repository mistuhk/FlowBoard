using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Resolves an @mention handle to a user within an organisation. A handle is matched against the
/// local-part of a member's email address (case-insensitively); resolution is scoped to active
/// members so handles are unambiguous. Implemented in the Identity Infrastructure layer.
/// </summary>
public interface IUserDirectory
{
    /// <summary>
    /// Returns the id of the organisation member whose email local-part matches the handle, or
    /// <c>null</c> if no active member matches or the handle is ambiguous (matched by more than one
    /// member), so an ambiguous mention never notifies the wrong person.
    /// </summary>
    /// <param name="organisationId">The organisation to resolve within.</param>
    /// <param name="handle">The @handle (without the leading @).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Guid?> ResolveHandleAsync(
        OrganisationId organisationId,
        string handle,
        CancellationToken cancellationToken = default);
}
