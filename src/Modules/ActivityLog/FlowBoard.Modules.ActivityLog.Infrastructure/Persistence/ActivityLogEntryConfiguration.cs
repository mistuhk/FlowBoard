using System.Text.Json;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.ActivityLog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.ActivityLog.Infrastructure.Persistence;

/// <summary>
/// EF Core configuration for <see cref="ActivityLogEntry"/>. The <c>activity_logs</c> table is
/// append-only: no <c>updated_at</c> (no trigger), no soft delete, no query filter. Metadata is
/// stored as JSONB.
/// </summary>
internal sealed class ActivityLogEntryConfiguration : IEntityTypeConfiguration<ActivityLogEntry>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ActivityLogEntry> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.OrganisationId)
            .HasColumnName("organisation_id")
            .HasConversion(id => id.Value, value => OrganisationId.From(value))
            .IsRequired();

        builder.Property(e => e.EntityType).HasColumnName("entity_type").IsRequired();
        builder.Property(e => e.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(e => e.EventType).HasColumnName("event_type").IsRequired();

        builder.Property(e => e.ActorId)
            .HasColumnName("actor_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? UserId.From(value.Value) : null);

        var metadataComparer = new ValueComparer<Dictionary<string, object>?>(
            (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
            value => value == null ? 0 : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => value);

        builder.Property(e => e.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(
                value => value == null ? null : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                json => json == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(json, (JsonSerializerOptions?)null))
            .Metadata.SetValueComparer(metadataComparer);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(e => new { e.OrganisationId, e.CreatedAt })
            .HasDatabaseName("ix_activity_logs_organisation_id");

        builder.HasIndex(e => new { e.OrganisationId, e.EntityType, e.EntityId, e.CreatedAt })
            .HasDatabaseName("ix_activity_logs_entity");

        builder.HasIndex(e => new { e.ActorId, e.CreatedAt })
            .HasDatabaseName("ix_activity_logs_actor_id");
    }
}
