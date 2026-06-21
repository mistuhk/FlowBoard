using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Projects.Domain.Aggregates;
using FlowBoard.Modules.Projects.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.Projects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <c>projects</c> table.
/// <para>
/// <c>updated_at</c> is database-managed by the <c>trg_projects_updated_at</c> trigger. Soft-deleted
/// projects are excluded by a global query filter. The table is tenant-scoped (Row-Level Security is
/// added in the migration), and queries are also scoped by <c>organisation_id</c> in the repository.
/// </para>
/// </summary>
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects", table =>
        {
            table.HasCheckConstraint("chk_projects_status", "status IN ('active', 'archived')");
            table.HasCheckConstraint("chk_projects_name", "char_length(name) BETWEEN 1 AND 150");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ProjectId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.OrganisationId)
            .HasColumnName("organisation_id")
            .HasConversion(id => id.Value, value => OrganisationId.From(value))
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .HasConversion(name => name.Value, value => ProjectName.FromPersistence(value))
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description");

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                status => status.Name.ToLowerInvariant(),
                value => ProjectStatus.FromPersistence(value))
            .IsRequired();

        builder.Property(p => p.CreatedById)
            .HasColumnName("created_by_id")
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(p => p.UpdatedAt).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(p => p.OrganisationId)
            .HasDatabaseName("ix_projects_organisation_id");

        builder.HasIndex(p => new { p.OrganisationId, p.Status })
            .HasDatabaseName("ix_projects_organisation_id_status")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(p => p.Id)
            .HasDatabaseName("ix_projects_active")
            .HasFilter("deleted_at IS NULL");

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
