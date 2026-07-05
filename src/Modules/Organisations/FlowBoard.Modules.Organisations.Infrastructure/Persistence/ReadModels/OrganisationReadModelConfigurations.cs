using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.Organisations.Infrastructure.Persistence.ReadModels;

/// <summary>
/// Maps the organisation read models with <c>ToSqlQuery</c> (query-only, excluded from migrations,
/// no shared-mapping conflict with the write aggregates or other modules' read models).
/// </summary>
internal sealed class OrganisationRowConfiguration : IEntityTypeConfiguration<OrganisationRow>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<OrganisationRow> builder) =>
        builder.HasNoKey().ToSqlQuery("SELECT id, name, slug, owner_id, deleted_at FROM organisations");
}

/// <summary>Backs <see cref="MembershipRow"/> with a defining query over the memberships table.</summary>
internal sealed class MembershipRowConfiguration : IEntityTypeConfiguration<MembershipRow>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<MembershipRow> builder) =>
        builder.HasNoKey().ToSqlQuery("SELECT organisation_id, user_id, role, joined_at FROM memberships");
}

/// <summary>Backs <see cref="UserRow"/> with a defining query over the users table.</summary>
internal sealed class UserRowConfiguration : IEntityTypeConfiguration<UserRow>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserRow> builder) =>
        builder.HasNoKey().ToSqlQuery("SELECT id, email, display_name FROM users");
}
