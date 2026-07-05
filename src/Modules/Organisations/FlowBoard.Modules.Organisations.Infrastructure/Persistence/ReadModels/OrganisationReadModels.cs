namespace FlowBoard.Modules.Organisations.Infrastructure.Persistence.ReadModels;

/// <summary>
/// Keyless read models backing the organisation read queries (CQRS read side). Each is mapped with
/// <c>ToSqlQuery</c> over an existing table so the queries compose in LINQ without depending on the
/// write aggregates or the Identity module's user aggregate.
/// </summary>
internal sealed class OrganisationRow
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public Guid OwnerId { get; init; }
    public DateTime? DeletedAt { get; init; }
}

/// <summary>Keyless read model over the <c>memberships</c> table.</summary>
internal sealed class MembershipRow
{
    public Guid OrganisationId { get; init; }
    public Guid UserId { get; init; }
    public string Role { get; init; } = null!;
    public DateTime JoinedAt { get; init; }
}

/// <summary>Keyless read model over the <c>users</c> table (id and display fields only).</summary>
internal sealed class UserRow
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
}
