using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <c>users</c> table.
/// <para>
/// <c>updated_at</c> is database-managed: the column defaults to <c>now()</c> on insert and
/// the <c>trg_users_updated_at</c> trigger (created in the migration) maintains it on update,
/// so EF never writes the value. Soft-deleted rows (<c>deleted_at</c> not null) are excluded
/// from all queries by a global filter.
/// </para>
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => UserId.From(value))
            .ValueGeneratedNever();  // assigned in the domain via User.Register

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .HasConversion(email => email.Value, value => Email.FromPersistence(value))
            .IsRequired();

        builder.Property(u => u.Password)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .HasConversion(password => password.Hash, value => HashedPassword.FromHash(value))
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .HasConversion(name => name.Value, value => DisplayName.FromPersistence(value))
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(2048)
            .HasConversion(
                uri => uri == null ? null : uri.ToString(),
                value => value == null ? null : new Uri(value));

        builder.Property(u => u.IsEmailVerified)
            .HasColumnName("is_email_verified")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Database-managed: default on insert, trg_users_updated_at on update. EF never writes it.
        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(u => u.UpdatedAt).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        // Soft delete: exclude deleted rows from every query.
        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
