using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Tasks.Application.Abstractions;
using FlowBoard.Modules.Tasks.Application.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Tasks.Infrastructure.Dashboard;

/// <summary>
/// Builds a user's dashboard from the keyless read models. Everything is user-scoped (assignee,
/// project membership, actor); the dashboard spans all organisations the user belongs to and is not
/// tenant-scoped. Uses <c>DateTime.UtcNow</c> as the reference point for overdue and 30-day windows.
/// </summary>
internal sealed class DashboardReader(AppDbContext context) : IDashboardReader
{
    private const string DoneStatus = "done";
    private const string ActiveProjectStatus = "active";
    private const int RecentActivityCount = 20;

    /// <inheritdoc/>
    public async Task<DashboardResponse> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddDays(-30);

        var assignedByStatus = (await context.Set<DashboardTask>().AsNoTracking()
                .Where(t => t.AssigneeId == userId && t.DeletedAt == null)
                .GroupBy(t => t.Status)
                .Select(g => new StatusCount(g.Key, g.Count()))
                .ToListAsync(cancellationToken))
            .OrderBy(s => s.Status)
            .ToList();

        var overdueCount = await context.Set<DashboardTask>().AsNoTracking()
            .CountAsync(t => t.AssigneeId == userId && t.DeletedAt == null
                && t.DueDate != null && t.DueDate < now && t.Status != DoneStatus, cancellationToken);

        // Active projects the user can see, following the app's visibility rule: org members
        // (owner/admin/member) see every project in that organisation; a Guest sees only the projects
        // they have been explicitly added to.
        var nonGuestOrgIds = await context.Set<DashboardMembership>().AsNoTracking()
            .Where(m => m.UserId == userId && (m.Role == "owner" || m.Role == "admin" || m.Role == "member"))
            .Select(m => m.OrganisationId)
            .ToListAsync(cancellationToken);
        var memberProjectIds = await context.Set<DashboardProjectMember>().AsNoTracking()
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync(cancellationToken);

        var activeProjects = await context.Set<DashboardProject>().AsNoTracking()
            .Where(p => p.Status == ActiveProjectStatus && p.DeletedAt == null
                && (nonGuestOrgIds.Contains(p.OrganisationId) || memberProjectIds.Contains(p.Id)))
            .Select(p => new DashboardProjectSummary(p.Id, p.Name, p.OrganisationId))
            .ToListAsync(cancellationToken);

        var recentActivity = await context.Set<DashboardActivity>().AsNoTracking()
            .Where(a => a.ActorId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(RecentActivityCount)
            .Select(a => new DashboardActivityItem(a.Id, a.EntityType, a.EntityId, a.EventType, a.CreatedAt))
            .ToListAsync(cancellationToken);

        // Completion rate: of tasks assigned to the user in the last 30 days, the fraction now done.
        var windowTotal = await context.Set<DashboardTask>().AsNoTracking()
            .CountAsync(t => t.AssigneeId == userId && t.DeletedAt == null && t.CreatedAt >= windowStart, cancellationToken);
        var windowDone = await context.Set<DashboardTask>().AsNoTracking()
            .CountAsync(t => t.AssigneeId == userId && t.DeletedAt == null && t.CreatedAt >= windowStart && t.Status == DoneStatus, cancellationToken);
        var completionRate = windowTotal == 0 ? 0d : (double)windowDone / windowTotal;

        // Workload: open (not done) tasks per assignee across the user's active projects.
        var activeProjectIds = activeProjects.Select(p => p.Id).ToList();
        var workload = activeProjectIds.Count == 0
            ? []
            : await context.Set<DashboardTask>().AsNoTracking()
                .Where(t => activeProjectIds.Contains(t.ProjectId) && t.DeletedAt == null
                    && t.Status != DoneStatus && t.AssigneeId != null)
                .GroupBy(t => t.AssigneeId!.Value)
                .Select(g => new AssigneeWorkload(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

        return new DashboardResponse(assignedByStatus, overdueCount, activeProjects, recentActivity, completionRate, workload);
    }
}
