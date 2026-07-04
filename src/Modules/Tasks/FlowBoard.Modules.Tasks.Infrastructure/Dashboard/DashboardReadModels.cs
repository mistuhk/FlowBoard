namespace FlowBoard.Modules.Tasks.Infrastructure.Dashboard;

/// <summary>
/// Keyless read models backing the dashboard (CQRS read side). Each maps to an existing table via
/// <c>ToView</c> for querying only, so the dashboard composes task, project, and activity data with
/// primitive columns (no value objects, no dependency on other modules' aggregates).
/// </summary>
internal sealed class DashboardTask
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid OrganisationId { get; init; }
    public Guid? AssigneeId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? DueDate { get; init; }
    public DateTime? DeletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>Keyless read model over the <c>projects</c> table.</summary>
internal sealed class DashboardProject
{
    public Guid Id { get; init; }
    public Guid OrganisationId { get; init; }
    public string Name { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime? DeletedAt { get; init; }
}

/// <summary>Keyless read model over the <c>project_members</c> table.</summary>
internal sealed class DashboardProjectMember
{
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>Keyless read model over the <c>memberships</c> table (organisation membership and role).</summary>
internal sealed class DashboardMembership
{
    public Guid OrganisationId { get; init; }
    public Guid UserId { get; init; }
    public string Role { get; init; } = null!;
}

/// <summary>Keyless read model over the <c>activity_logs</c> table.</summary>
internal sealed class DashboardActivity
{
    public Guid Id { get; init; }
    public Guid? ActorId { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public string EventType { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}
