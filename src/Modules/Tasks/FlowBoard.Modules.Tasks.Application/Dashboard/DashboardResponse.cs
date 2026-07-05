namespace FlowBoard.Modules.Tasks.Application.Dashboard;

/// <summary>The current user's dashboard: a read-optimised snapshot of their work across organisations.</summary>
/// <param name="AssignedByStatus">Count of the user's active assigned tasks, per status.</param>
/// <param name="OverdueCount">Number of the user's assigned tasks that are past due and not done.</param>
/// <param name="ActiveProjects">Active projects the user is a member of.</param>
/// <param name="RecentActivity">The user's 20 most recent activity events, newest first.</param>
/// <param name="CompletionRate">
/// Of the tasks assigned to the user in the last 30 days, the fraction now done (0 to 1).
/// </param>
/// <param name="Workload">Open task count per assignee across the user's active projects.</param>
public sealed record DashboardResponse(
    IReadOnlyList<StatusCount> AssignedByStatus,
    int OverdueCount,
    IReadOnlyList<DashboardProjectSummary> ActiveProjects,
    IReadOnlyList<DashboardActivityItem> RecentActivity,
    double CompletionRate,
    IReadOnlyList<AssigneeWorkload> Workload);

/// <summary>A task count for a status.</summary>
/// <param name="Status">The status token (for example <c>todo</c>).</param>
/// <param name="Count">The number of tasks in that status.</param>
public sealed record StatusCount(string Status, int Count);

/// <summary>A brief summary of an active project.</summary>
/// <param name="Id">The project id.</param>
/// <param name="Name">The project name.</param>
/// <param name="OrganisationId">The organisation the project belongs to.</param>
public sealed record DashboardProjectSummary(Guid Id, string Name, Guid OrganisationId);

/// <summary>A recent activity event.</summary>
/// <param name="Id">The entry id.</param>
/// <param name="EntityType">The entity acted upon.</param>
/// <param name="EntityId">The entity id.</param>
/// <param name="EventType">The event token.</param>
/// <param name="CreatedAt">When it occurred.</param>
public sealed record DashboardActivityItem(Guid Id, string EntityType, Guid EntityId, string EventType, DateTime CreatedAt);

/// <summary>Open task count for one assignee.</summary>
/// <param name="AssigneeId">The assignee.</param>
/// <param name="OpenTasks">Their count of open (not done) tasks in the user's active projects.</param>
public sealed record AssigneeWorkload(Guid AssigneeId, int OpenTasks);
