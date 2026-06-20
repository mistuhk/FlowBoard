using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.Organisations.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <c>organisations</c> table and its owned <c>memberships</c>
/// child collection.
/// <para>
/// <c>updated_at</c> on <c>organisations</c> is database-managed: the column defaults to
/// <c>now()</c> on insert and the <c>trg_organisations_updated_at</c> trigger (created in the
/// migration) maintains it on update, so EF never writes the value. Soft-deleted organisations
/// (<c>deleted_at</c> not null) are excluded from all queries by a global filter. Memberships are
/// owned by the aggregate: they are loaded with their organisation and never queried directly.
/// </para>
/// </summary>
public sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisations", table =>
        {
            table.HasCheckConstraint("chk_organisations_name", "char_length(name) BETWEEN 2 AND 100");
            table.HasCheckConstraint("chk_organisations_slug", "slug ~ '^[a-z0-9-]+$'");
        });

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => OrganisationId.From(value))
            .ValueGeneratedNever();  // assigned in the domain via Organisation.Create

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .HasConversion(name => name.Value, value => OrganisationName.FromPersistence(value))
            .IsRequired();

        builder.Property(o => o.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .HasConversion(slug => slug.Value, value => OrganisationSlug.FromPersistence(value))
            .IsRequired();

        builder.Property(o => o.OwnerId)
            .HasColumnName("owner_id")
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Database-managed: default on insert, trg_organisations_updated_at on update. EF never writes it.
        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(o => o.UpdatedAt).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(o => o.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(o => o.Slug)
            .IsUnique()
            .HasDatabaseName("ix_organisations_slug");

        builder.HasIndex(o => o.OwnerId)
            .HasDatabaseName("ix_organisations_owner_id");

        builder.HasIndex(o => o.Id)
            .HasDatabaseName("ix_organisations_active")
            .HasFilter("deleted_at IS NULL");

        // Soft delete: exclude deleted organisations from every query.
        builder.HasQueryFilter(o => o.DeletedAt == null);

        ConfigureMemberships(builder);

        // Load memberships with their organisation; the aggregate owns the collection.
        builder.Navigation(o => o.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }

    private static void ConfigureMemberships(EntityTypeBuilder<Organisation> builder)
    {
        builder.OwnsMany(o => o.Memberships, membership =>
        {
            membership.ToTable("memberships", table =>
                table.HasCheckConstraint(
                    "chk_memberships_role",
                    "role IN ('owner', 'admin', 'member', 'guest')"));

            membership.HasKey(m => m.Id);

            membership.Property(m => m.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => MembershipId.From(value))
                .ValueGeneratedNever();

            // organisation_id is the foreign key back to the owning aggregate.
            membership.WithOwner().HasForeignKey(m => m.OrganisationId);
            membership.Property(m => m.OrganisationId)
                .HasColumnName("organisation_id")
                .HasConversion(id => id.Value, value => OrganisationId.From(value));

            membership.Property(m => m.UserId)
                .HasColumnName("user_id")
                .HasConversion(id => id.Value, value => UserId.From(value))
                .IsRequired();

            membership.Property(m => m.Role)
                .HasColumnName("role")
                .HasMaxLength(20)
                .HasConversion(
                    role => role.Name.ToLowerInvariant(),
                    value => MemberRole.FromPersistence(value))
                .IsRequired();

            membership.Property(m => m.InvitedById)
                .HasColumnName("invited_by_id")
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    value => value.HasValue ? UserId.From(value.Value) : null);

            membership.Property(m => m.JoinedAt)
                .HasColumnName("joined_at")
                .IsRequired();

            // Memberships have no created_at column; joined_at is the temporal anchor.
            membership.Ignore(m => m.CreatedAt);

            membership.HasIndex(m => m.UserId)
                .HasDatabaseName("ix_memberships_user_id");

            membership.HasIndex(m => m.OrganisationId)
                .HasDatabaseName("ix_memberships_organisation_id");

            membership.HasIndex(m => new { m.OrganisationId, m.Role })
                .HasDatabaseName("ix_memberships_organisation_id_role");

            membership.HasIndex(m => new { m.UserId, m.OrganisationId })
                .IsUnique()
                .HasDatabaseName("ix_memberships_user_id_organisation_id");
        });
    }
}
