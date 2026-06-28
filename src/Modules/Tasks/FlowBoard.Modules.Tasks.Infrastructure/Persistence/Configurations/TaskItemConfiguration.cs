using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.Tasks.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <c>tasks</c> table.
/// <para>
/// <c>updated_at</c> is database-managed by the <c>trg_tasks_updated_at</c> trigger; soft-deleted
/// tasks are excluded by a global query filter. The table is tenant-scoped (RLS added in the
/// migration), and queries are also scoped by <c>organisation_id</c> in the repository. The
/// <c>search_vector</c> generated column is created in the migration and is not part of the model.
/// </para>
/// </summary>
public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks", table =>
        {
            table.HasCheckConstraint("chk_tasks_status", "status IN ('todo', 'in_progress', 'blocked', 'done')");
            table.HasCheckConstraint("chk_tasks_priority", "priority IN ('low', 'medium', 'high', 'critical')");
            table.HasCheckConstraint("chk_tasks_title", "char_length(title) BETWEEN 1 AND 255");
        });

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => TaskId.From(value))
            .ValueGeneratedNever();

        builder.Property(t => t.ProjectId)
            .HasColumnName("project_id")
            .HasConversion(id => id.Value, value => ProjectId.From(value))
            .IsRequired();

        builder.Property(t => t.OrganisationId)
            .HasColumnName("organisation_id")
            .HasConversion(id => id.Value, value => OrganisationId.From(value))
            .IsRequired();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(status => status.DbValue, value => TaskItemStatus.FromPersistence(value))
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20)
            .HasConversion(priority => priority.Name.ToLowerInvariant(), value => Priority.FromPersistence(value))
            .IsRequired();

        builder.Property(t => t.AssigneeId)
            .HasColumnName("assignee_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? UserId.From(value.Value) : null);

        builder.Property(t => t.CreatedById)
            .HasColumnName("created_by_id")
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(t => t.UpdatedAt).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(t => t.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(t => t.ProjectId)
            .HasDatabaseName("ix_tasks_project_id")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(t => t.OrganisationId)
            .HasDatabaseName("ix_tasks_organisation_id")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(t => t.AssigneeId)
            .HasDatabaseName("ix_tasks_assignee_id")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(t => new { t.OrganisationId, t.Status })
            .HasDatabaseName("ix_tasks_organisation_id_status")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(t => new { t.OrganisationId, t.Priority })
            .HasDatabaseName("ix_tasks_organisation_id_priority")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(t => t.DueDate)
            .HasDatabaseName("ix_tasks_due_date")
            .HasFilter("due_date IS NOT NULL AND deleted_at IS NULL");

        builder.HasQueryFilter(t => t.DeletedAt == null);

        ConfigureComments(builder);
        ConfigureAttachments(builder);

        // Load comments and attachments with their task; the aggregate owns both collections.
        // Soft-deleted children are loaded too (owned types cannot carry a query filter); callers exclude them.
        builder.Navigation(t => t.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(t => t.Attachments)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }

    private static void ConfigureAttachments(EntityTypeBuilder<TaskItem> builder)
    {
        builder.OwnsMany(t => t.Attachments, attachment =>
        {
            attachment.ToTable("file_attachments", table =>
                table.HasCheckConstraint("chk_file_attachments_size", "file_size_bytes > 0"));

            attachment.HasKey(a => a.Id);

            attachment.Property(a => a.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => AttachmentId.From(value))
                .ValueGeneratedNever();

            attachment.WithOwner().HasForeignKey(a => a.TaskId);
            attachment.Property(a => a.TaskId)
                .HasColumnName("task_id")
                .HasConversion(id => id.Value, value => TaskId.From(value));

            attachment.Property(a => a.OrganisationId)
                .HasColumnName("organisation_id")
                .HasConversion(id => id.Value, value => OrganisationId.From(value))
                .IsRequired();

            attachment.Property(a => a.UploadedById)
                .HasColumnName("uploaded_by_id")
                .HasConversion(id => id.Value, value => UserId.From(value))
                .IsRequired();

            attachment.Property(a => a.FileName).HasColumnName("file_name").IsRequired();
            attachment.Property(a => a.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired();
            attachment.Property(a => a.MimeType).HasColumnName("mime_type").IsRequired();
            attachment.Property(a => a.StorageKey).HasColumnName("storage_key").IsRequired();
            attachment.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
            attachment.Property(a => a.DeletedAt).HasColumnName("deleted_at");

            attachment.HasIndex(a => a.TaskId)
                .HasDatabaseName("ix_file_attachments_task_id")
                .HasFilter("deleted_at IS NULL");

            attachment.HasIndex(a => a.OrganisationId)
                .HasDatabaseName("ix_file_attachments_organisation_id");
        });
    }

    private static void ConfigureComments(EntityTypeBuilder<TaskItem> builder)
    {
        builder.OwnsMany(t => t.Comments, comment =>
        {
            comment.ToTable("comments", table =>
                table.HasCheckConstraint("chk_comments_content", "char_length(content) BETWEEN 1 AND 10000"));

            comment.HasKey(c => c.Id);

            comment.Property(c => c.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => CommentId.From(value))
                .ValueGeneratedNever();

            // task_id is the foreign key back to the owning aggregate.
            comment.WithOwner().HasForeignKey(c => c.TaskId);
            comment.Property(c => c.TaskId)
                .HasColumnName("task_id")
                .HasConversion(id => id.Value, value => TaskId.From(value));

            comment.Property(c => c.OrganisationId)
                .HasColumnName("organisation_id")
                .HasConversion(id => id.Value, value => OrganisationId.From(value))
                .IsRequired();

            comment.Property(c => c.AuthorId)
                .HasColumnName("author_id")
                .HasConversion(id => id.Value, value => UserId.From(value))
                .IsRequired();

            comment.Property(c => c.Content)
                .HasColumnName("content")
                .HasConversion(content => content.Value, value => CommentContent.FromPersistence(value))
                .IsRequired();

            comment.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            comment.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAddOrUpdate();
            comment.Property(c => c.UpdatedAt).Metadata
                .SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            comment.Property(c => c.DeletedAt)
                .HasColumnName("deleted_at");

            comment.HasIndex(c => c.TaskId)
                .HasDatabaseName("ix_comments_task_id")
                .HasFilter("deleted_at IS NULL");

            comment.HasIndex(c => c.OrganisationId)
                .HasDatabaseName("ix_comments_organisation_id");

            comment.HasIndex(c => c.AuthorId)
                .HasDatabaseName("ix_comments_author_id");
        });
    }
}
