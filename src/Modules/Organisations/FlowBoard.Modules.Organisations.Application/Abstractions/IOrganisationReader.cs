namespace FlowBoard.Modules.Organisations.Application.Abstractions;

/// <summary>
/// Read-only queries over organisations and their members (CQRS read side), implemented in the
/// Organisations Infrastructure layer. Composes organisation, membership, and user data without
/// depending on the write aggregates or the Identity module.
/// </summary>
public interface IOrganisationReader
{
    /// <summary>Lists the organisations the user belongs to, with the user's role in each.</summary>
    /// <param name="userId">The user whose organisations to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<OrganisationSummaryResponse>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single organisation the user belongs to, with their role, or <c>null</c> if the user
    /// is not a member (or it does not exist or is soft-deleted).
    /// </summary>
    /// <param name="userId">The requesting user.</param>
    /// <param name="organisationId">The organisation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<OrganisationSummaryResponse?> GetForUserAsync(Guid userId, Guid organisationId, CancellationToken cancellationToken = default);

    /// <summary>Lists the members of an organisation, ordered by join date.</summary>
    /// <param name="organisationId">The organisation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<MemberResponse>> ListMembersAsync(Guid organisationId, CancellationToken cancellationToken = default);
}
