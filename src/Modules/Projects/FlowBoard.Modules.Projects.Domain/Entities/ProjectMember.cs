using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Projects.Domain.Entities;

/// <summary>
/// Explicit membership of a project, used for Guest access control. A child entity of the
/// <see cref="Aggregates.Project"/> aggregate: it is created and removed only through the root.
/// </summary>
public sealed class ProjectMember : Entity<ProjectMemberId>
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private ProjectMember() { }

    private ProjectMember(ProjectMemberId id, ProjectId projectId, UserId userId, DateTime addedAt) : base(id)
    {
        ProjectId = projectId;
        UserId = userId;
        AddedAt = addedAt;
    }

    /// <summary>The project this membership belongs to.</summary>
    public ProjectId ProjectId { get; private set; }

    /// <summary>The user who is a member of the project.</summary>
    public UserId UserId { get; private set; }

    /// <summary>UTC timestamp at which the user was added to the project.</summary>
    public DateTime AddedAt { get; private set; }

    /// <summary>Creates a new project membership. Internal to the aggregate.</summary>
    /// <param name="projectId">The project being joined.</param>
    /// <param name="userId">The user being added.</param>
    /// <returns>A new <see cref="ProjectMember"/>.</returns>
    internal static ProjectMember Create(ProjectId projectId, UserId userId) =>
        new(ProjectMemberId.New(), projectId, userId, DateTime.UtcNow);
}
