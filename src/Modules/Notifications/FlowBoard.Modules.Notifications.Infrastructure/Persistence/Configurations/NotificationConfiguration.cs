using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Domain.Aggregates;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.Notifications.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <c>notifications</c> table. Notifications are append-only apart
/// from the <c>is_read</c> flag: there is no <c>updated_at</c> column and no soft delete. The table
/// is tenant-scoped (RLS added in the migration).
/// </summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", table =>
            table.HasCheckConstraint(
                "chk_notifications_type",
                "type IN ('task_assigned', 'user_mentioned', 'project_invited', 'task_status_changed', 'comment_added', 'task_blocked')"));

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => NotificationId.From(value))
            .ValueGeneratedNever();

        builder.Property(n => n.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(n => n.OrganisationId)
            .HasColumnName("organisation_id")
            .HasConversion(id => id.Value, value => OrganisationId.From(value))
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasMaxLength(30)
            .HasConversion(type => type.DbValue, value => NotificationType.FromPersistence(value))
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .IsRequired();

        builder.Property(n => n.EntityType)
            .HasColumnName("entity_type");

        builder.Property(n => n.EntityId)
            .HasColumnName("entity_id");

        builder.Property(n => n.IsRead)
            .HasColumnName("is_read")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Two indexes over the same columns (named explicitly so EF keeps both): one for all of a
        // user's notifications, and a partial one for the common unread query.
        builder.HasIndex(["UserId", "CreatedAt"], "user_all")
            .IsDescending(false, true)
            .HasDatabaseName("ix_notifications_user_id");

        builder.HasIndex(["UserId", "CreatedAt"], "user_unread")
            .IsDescending(false, true)
            .HasFilter("is_read = false")
            .HasDatabaseName("ix_notifications_user_unread");
    }
}
