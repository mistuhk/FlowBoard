using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Organisations.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOrganisationRepository"/>. Writes are buffered on the
/// shared <see cref="AppDbContext"/> and flushed by the unit of work when the surrounding
/// transaction commits. Memberships are loaded automatically with their organisation.
/// </summary>
internal sealed class OrganisationRepository(AppDbContext context) : IOrganisationRepository
{
    /// <inheritdoc/>
    public Task<bool> ExistsBySlugAsync(OrganisationSlug slug, CancellationToken cancellationToken = default) =>
        context.Set<Organisation>().AnyAsync(o => o.Slug == slug, cancellationToken);

    /// <inheritdoc/>
    public Task<Organisation?> GetByIdAsync(OrganisationId id, CancellationToken cancellationToken = default) =>
        context.Set<Organisation>().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    /// <inheritdoc/>
    public Task<Organisation?> GetByInvitationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        context.Set<Organisation>()
            .FirstOrDefaultAsync(o => o.Invitations.Any(i => i.TokenHash == tokenHash), cancellationToken);

    /// <inheritdoc/>
    public async Task AddAsync(Organisation organisation, CancellationToken cancellationToken = default) =>
        await context.Set<Organisation>().AddAsync(organisation, cancellationToken);
}
