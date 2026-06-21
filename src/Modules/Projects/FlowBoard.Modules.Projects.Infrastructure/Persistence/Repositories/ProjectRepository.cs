using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Projects.Domain.Aggregates;
using FlowBoard.Modules.Projects.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Projects.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IProjectRepository"/>. Every lookup is scoped to an
/// organisation, the primary tenant-isolation mechanism for reads (Row-Level Security is a
/// database-level defence-in-depth on top).
/// </summary>
internal sealed class ProjectRepository(AppDbContext context) : IProjectRepository
{
    /// <inheritdoc/>
    public Task<Project?> GetByIdAsync(
        ProjectId id,
        OrganisationId organisationId,
        CancellationToken cancellationToken = default) =>
        context.Set<Project>()
            .FirstOrDefaultAsync(p => p.Id == id && p.OrganisationId == organisationId, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Project>> ListByOrganisationAsync(
        OrganisationId organisationId,
        CancellationToken cancellationToken = default) =>
        await context.Set<Project>()
            .AsNoTracking()
            .Where(p => p.OrganisationId == organisationId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task AddAsync(Project project, CancellationToken cancellationToken = default) =>
        await context.Set<Project>().AddAsync(project, cancellationToken);
}
