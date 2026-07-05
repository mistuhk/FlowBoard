using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Organisations.Application;
using FlowBoard.Modules.Organisations.Application.Abstractions;
using FlowBoard.Modules.Organisations.Infrastructure.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Organisations.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IOrganisationReader"/> over the keyless read models. Each
/// relation is queried on its own (filtering and <c>Contains</c> translate, but EF cannot compose a
/// server-side join across two <c>ToSqlQuery</c> sources), then combined in memory. The result sets
/// are small (a user's organisations, an organisation's members).
/// </summary>
internal sealed class OrganisationReader(AppDbContext context) : IOrganisationReader
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<OrganisationSummaryResponse>> ListForUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var memberships = await context.Set<MembershipRow>().AsNoTracking()
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);
        if (memberships.Count == 0)
            return [];

        var orgIds = memberships.Select(m => m.OrganisationId).ToList();
        var orgs = await context.Set<OrganisationRow>().AsNoTracking()
            .Where(o => o.DeletedAt == null && orgIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        var roleByOrg = memberships.ToDictionary(m => m.OrganisationId, m => m.Role);
        return orgs
            .Select(o => new OrganisationSummaryResponse(o.Id, o.Name, o.Slug, o.OwnerId, roleByOrg[o.Id]))
            .OrderBy(o => o.Name)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<OrganisationSummaryResponse?> GetForUserAsync(
        Guid userId, Guid organisationId, CancellationToken cancellationToken = default)
    {
        var membership = await context.Set<MembershipRow>().AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId && m.OrganisationId == organisationId, cancellationToken);
        if (membership is null)
            return null;

        var org = await context.Set<OrganisationRow>().AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organisationId && o.DeletedAt == null, cancellationToken);
        return org is null
            ? null
            : new OrganisationSummaryResponse(org.Id, org.Name, org.Slug, org.OwnerId, membership.Role);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MemberResponse>> ListMembersAsync(
        Guid organisationId, CancellationToken cancellationToken = default)
    {
        var memberships = await context.Set<MembershipRow>().AsNoTracking()
            .Where(m => m.OrganisationId == organisationId)
            .ToListAsync(cancellationToken);
        if (memberships.Count == 0)
            return [];

        var userIds = memberships.Select(m => m.UserId).ToList();
        var users = await context.Set<UserRow>().AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(cancellationToken);
        var userById = users.ToDictionary(u => u.Id);

        return memberships
            .Where(m => userById.ContainsKey(m.UserId))
            .Select(m =>
            {
                var user = userById[m.UserId];
                return new MemberResponse(user.Id, user.Email, user.DisplayName, m.Role, m.JoinedAt);
            })
            .OrderBy(m => m.JoinedAt)
            .ToList();
    }
}
