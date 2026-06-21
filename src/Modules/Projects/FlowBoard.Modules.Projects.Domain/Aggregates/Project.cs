using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Events;
using FlowBoard.Modules.Projects.Domain.ValueObjects;

namespace FlowBoard.Modules.Projects.Domain.Aggregates;

/// <summary>
/// Aggregate root for the Projects bounded context. A project belongs to exactly one organisation
/// and that never changes. An archived project is read-only: its details cannot be edited until it
/// is restored.
/// </summary>
public sealed class Project : AggregateRoot<ProjectId>
{
    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private Project() { }

    /// <summary>The organisation that owns the project. Fixed for the project's lifetime.</summary>
    public OrganisationId OrganisationId { get; private set; }

    /// <summary>The project's name.</summary>
    public ProjectName Name { get; private set; } = null!;

    /// <summary>An optional free-text description.</summary>
    public string? Description { get; private set; }

    /// <summary>The project's lifecycle status.</summary>
    public ProjectStatus Status { get; private set; } = null!;

    /// <summary>The user who created the project.</summary>
    public UserId CreatedById { get; private set; }

    /// <summary>
    /// UTC timestamp of the most recent change. Database-managed by the <c>updated_at</c> trigger;
    /// never assigned in code.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>UTC timestamp at which the project was soft-deleted. <c>null</c> if active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Whether the project is archived.</summary>
    public bool IsArchived => Status == ProjectStatus.Archived;

    /// <summary>
    /// Creates a new active project and raises <see cref="ProjectCreatedEvent"/>.
    /// </summary>
    /// <param name="organisationId">The owning organisation.</param>
    /// <param name="name">The project name.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="createdById">The user creating the project.</param>
    /// <returns>A new active <see cref="Project"/>.</returns>
    public static Project Create(
        OrganisationId organisationId,
        ProjectName name,
        string? description,
        UserId createdById)
    {
        var project = new Project
        {
            Id = ProjectId.New(),
            OrganisationId = organisationId,
            Name = name,
            Description = description?.Trim(),
            Status = ProjectStatus.Active,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow,  // object initialiser bypasses the base ctor
            // updated_at is database-managed (column default on insert, trigger on update).
        };

        project.Raise(new ProjectCreatedEvent(project.Id, organisationId, createdById));
        return project;
    }

    /// <summary>Updates the project's editable details. Rejected while the project is archived.</summary>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <exception cref="DomainException">Thrown if the project is archived.</exception>
    public void Update(ProjectName name, string? description)
    {
        if (IsArchived)
            throw new DomainException("An archived project cannot be edited. Restore it first.");

        Name = name;
        Description = description?.Trim();
    }

    /// <summary>
    /// Archives the project and raises <see cref="ProjectArchivedEvent"/>. Idempotent: archiving an
    /// already-archived project is a no-op and raises no event.
    /// </summary>
    /// <param name="archivedById">The user archiving the project.</param>
    public void Archive(UserId archivedById)
    {
        if (IsArchived)
            return;

        Status = ProjectStatus.Archived;
        Raise(new ProjectArchivedEvent(Id, OrganisationId, archivedById));
    }

    /// <summary>
    /// Restores an archived project and raises <see cref="ProjectRestoredEvent"/>. Idempotent:
    /// restoring an active project is a no-op and raises no event.
    /// </summary>
    /// <param name="restoredById">The user restoring the project.</param>
    public void Restore(UserId restoredById)
    {
        if (!IsArchived)
            return;

        Status = ProjectStatus.Active;
        Raise(new ProjectRestoredEvent(Id, OrganisationId));
    }

    /// <summary>
    /// Soft-deletes the project and raises <see cref="ProjectDeletedEvent"/>. Idempotent.
    /// </summary>
    /// <param name="deletedById">The user deleting the project.</param>
    public void Delete(UserId deletedById)
    {
        if (DeletedAt is not null)
            return;

        DeletedAt = DateTime.UtcNow;
        Raise(new ProjectDeletedEvent(Id, OrganisationId));
    }
}
