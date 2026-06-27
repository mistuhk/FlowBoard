using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IUserDirectory"/>. Resolves a mention handle to an active
/// organisation member by matching the handle against the email local-part. The join spans the
/// Identity-owned <c>users</c> table and the Organisations-owned <c>memberships</c> table, so it is
/// expressed as parameterised raw SQL rather than referencing both modules' entity types.
/// </summary>
internal sealed class UserDirectory(AppDbContext context) : IUserDirectory
{
    /// <inheritdoc/>
    public async Task<Guid?> ResolveHandleAsync(
        OrganisationId organisationId,
        string handle,
        CancellationToken cancellationToken = default)
    {
        var normalisedHandle = handle.Trim().ToLowerInvariant();

        // Fetch up to two matches: if more than one member shares the local-part the handle is
        // ambiguous, so we resolve to nobody rather than notifying an arbitrary person.
        var matches = await context.Database
            .SqlQuery<Guid>(
                $"""
                 SELECT u.id AS "Value"
                 FROM users u
                 JOIN memberships m ON m.user_id = u.id
                 WHERE m.organisation_id = {organisationId.Value}
                   AND u.deleted_at IS NULL
                   AND lower(split_part(u.email, '@', 1)) = {normalisedHandle}
                 LIMIT 2
                 """)
            .ToListAsync(cancellationToken);

        return matches.Count == 1 ? matches[0] : null;
    }
}
