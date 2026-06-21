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

        ConfigureMembers(builder);

        // Load the project's members with the project; the aggregate owns the collection.
        builder.Navigation(p => p.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }

    private static void ConfigureMembers(EntityTypeBuilder<Project> builder)
    {
        builder.OwnsMany(p => p.Members, member =>
        {
            member.ToTable("project_members");

            member.HasKey(m => m.Id);

            member.Property(m => m.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => ProjectMemberId.From(value))
                .ValueGeneratedNever();

            member.WithOwner().HasForeignKey(m => m.ProjectId);
            member.Property(m => m.ProjectId)
                .HasColumnName("project_id")
                .HasConversion(id => id.Value, value => ProjectId.From(value));

            member.Property(m => m.UserId)
                .HasColumnName("user_id")
                .HasConversion(id => id.Value, value => UserId.From(value))
                .IsRequired();

            member.Property(m => m.AddedAt)
                .HasColumnName("added_at")
                .IsRequired();

            // project_members has no created_at column; added_at is the temporal anchor.
            member.Ignore(m => m.CreatedAt);

            member.HasIndex(m => m.ProjectId)
                .HasDatabaseName("ix_project_members_project_id");

            member.HasIndex(m => m.UserId)
                .HasDatabaseName("ix_project_members_user_id");

            member.HasIndex(m => new { m.ProjectId, m.UserId })
                .IsUnique()
                .HasDatabaseName("ix_project_members_project_id_user_id");
        });
    }
}
