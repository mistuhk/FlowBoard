using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Entities;
using FlowBoard.Modules.Organisations.Domain.Events;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Aggregates;

/// <summary>
/// Aggregate root for the Organisations bounded context.
/// Owns its <see cref="Membership"/> children and enforces every membership invariant,
/// the central one being that an organisation always has exactly one owner.
/// All membership state is reached through this root, never mutated directly.
/// </summary>
public sealed class Organisation : AggregateRoot<OrganisationId>
{
    private readonly List<Membership> _memberships = [];

    /// <summary>Parameterless constructor required for EF Core materialisation.</summary>
    private Organisation() { }

    /// <summary>The organisation's display name.</summary>
    public OrganisationName Name { get; private set; } = null!;

    /// <summary>The organisation's unique, URL-safe slug.</summary>
    public OrganisationSlug Slug { get; private set; } = null!;

    /// <summary>The user who currently owns the organisation.</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>The organisation's memberships. Read-only: mutated only through aggregate behaviour.</summary>
    public IReadOnlyList<Membership> Memberships => _memberships.AsReadOnly();

    /// <summary>
    /// UTC timestamp of the most recent change to this aggregate.
    /// Database-managed: set on insert by the <c>updated_at</c> column default and on
    /// update by the organisation's <c>updated_at</c> trigger. Never assigned in code.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>UTC timestamp at which this organisation was soft-deleted. <c>null</c> if active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Creates a new organisation, registering the creator as its sole owner, and raises
    /// <see cref="OrganisationCreatedEvent"/>.
    /// </summary>
    /// <param name="name">The organisation's display name.</param>
    /// <param name="slug">The organisation's unique, URL-safe slug.</param>
    /// <param name="ownerId">The user creating the organisation, who becomes its owner.</param>
    /// <returns>A new <see cref="Organisation"/> with a single owner membership.</returns>
    public static Organisation Create(OrganisationName name, OrganisationSlug slug, UserId ownerId)
    {
        var organisation = new Organisation
        {
            Id = OrganisationId.New(),
            Name = name,
            Slug = slug,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,  // object initialiser bypasses the base ctor, so set it explicitly
            // updated_at is database-managed (column default on insert, trigger on update).
        };

        organisation._memberships.Add(
            Membership.Create(organisation.Id, ownerId, MemberRole.Owner, invitedById: null));

        organisation.Raise(new OrganisationCreatedEvent(organisation.Id, ownerId));
        return organisation;
    }

    /// <summary>
    /// Renames the organisation. Only the owner may rename it. The new name is already
    /// validated by <see cref="OrganisationName"/>.
    /// </summary>
    /// <param name="name">The new organisation name.</param>
    /// <param name="renamedById">The user attempting the rename.</param>
    /// <exception cref="ForbiddenException">Thrown if the caller is not the owner.</exception>
    public void Rename(OrganisationName name, UserId renamedById)
    {
        if (renamedById != OwnerId)
            throw new ForbiddenException("Only the organisation owner may rename the organisation.");

        Name = name;
    }

    /// <summary>
    /// Soft-deletes the organisation and raises <see cref="OrganisationDeletedEvent"/>.
    /// Only the owner may delete the organisation. Idempotent: deleting an already-deleted
    /// organisation is a no-op and raises no event.
    /// </summary>
    /// <param name="deletedById">The user attempting the deletion.</param>
    /// <exception cref="ForbiddenException">Thrown if the caller is not the owner.</exception>
    public void Delete(UserId deletedById)
    {
        if (DeletedAt is not null)
            return;

        if (deletedById != OwnerId)
            throw new ForbiddenException("Only the organisation owner may delete the organisation.");

        DeletedAt = DateTime.UtcNow;
        Raise(new OrganisationDeletedEvent(Id));
    }
}
