using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Projects.Domain.Aggregates;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Projects.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IProjectReader"/>. Soft-deleted projects are excluded by the
/// global query filter, so a missing row maps to <see cref="ProjectAvailability.NotFound"/>.
/// </summary>
internal sealed class ProjectReader(AppDbContext context) : IProjectReader
{
    /// <inheritdoc/>
    public async Task<ProjectAvailability> GetAvailabilityAsync(
        ProjectId projectId,
        OrganisationId organisationId,
        CancellationToken cancellationToken = default)
    {
        var project = await context.Set<Project>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OrganisationId == organisationId, cancellationToken);

        if (project is null)
            return ProjectAvailability.NotFound;

        return project.Status == ProjectStatus.Archived
            ? ProjectAvailability.Archived
            : ProjectAvailability.Active;
    }
}
