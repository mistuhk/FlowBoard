using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.Tasks.Infrastructure.Dashboard;

/// <summary>
/// Maps the dashboard's keyless read models with <c>ToSqlQuery</c>: each is backed by its own defining
/// SELECT over an existing table. This keeps them query-only (never migrated), independent of the
/// owning modules' aggregates, and free of the shared-mapping conflict that arises when several
/// keyless entities target the same table or view. LINQ composes on top of the defining query.
/// </summary>
internal sealed class DashboardTaskConfiguration : IEntityTypeConfiguration<DashboardTask>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<DashboardTask> builder) =>
        builder.HasNoKey().ToSqlQuery(
            "SELECT id, project_id, organisation_id, assignee_id, status, due_date, deleted_at, created_at, updated_at FROM tasks");
}

/// <summary>Backs <see cref="DashboardProject"/> with a defining query over the projects table.</summary>
internal sealed class DashboardProjectConfiguration : IEntityTypeConfiguration<DashboardProject>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<DashboardProject> builder) =>
        builder.HasNoKey().ToSqlQuery("SELECT id, organisation_id, name, status, deleted_at FROM projects");
}

/// <summary>Backs <see cref="DashboardProjectMember"/> with a defining query over the project_members table.</summary>
internal sealed class DashboardProjectMemberConfiguration : IEntityTypeConfiguration<DashboardProjectMember>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<DashboardProjectMember> builder) =>
        builder.HasNoKey().ToSqlQuery("SELECT project_id, user_id FROM project_members");
}

/// <summary>Backs <see cref="DashboardActivity"/> with a defining query over the activity_logs table.</summary>
internal sealed class DashboardActivityConfiguration : IEntityTypeConfiguration<DashboardActivity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<DashboardActivity> builder) =>
        builder.HasNoKey().ToSqlQuery("SELECT id, actor_id, entity_type, entity_id, event_type, created_at FROM activity_logs");
}

/// <summary>Backs <see cref="DashboardMembership"/> with a defining query over the memberships table.</summary>
internal sealed class DashboardMembershipConfiguration : IEntityTypeConfiguration<DashboardMembership>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<DashboardMembership> builder) =>
        builder.HasNoKey().ToSqlQuery("SELECT organisation_id, user_id, role FROM memberships");
}
