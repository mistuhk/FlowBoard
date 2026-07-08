using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Organisations.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IOrganisationMembershipReader"/>. Loads the organisation
/// with its memberships (soft-deleted organisations are excluded by the global query filter) and
/// resolves the caller's role in memory.
/// </summary>
internal sealed class OrganisationMembershipReader(AppDbContext context) : IOrganisationMembershipReader
{
    /// <inheritdoc/>
    public async Task<string?> GetRoleNameAsync(
        OrganisationId organisationId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var organisation = await context.Set<Organisation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organisationId, cancellationToken);

        return organisation?.Memberships
            .FirstOrDefault(m => m.UserId == userId)?
            .Role.Name;
    }
}
