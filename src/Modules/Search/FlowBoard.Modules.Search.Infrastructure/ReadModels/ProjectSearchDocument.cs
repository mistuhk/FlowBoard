namespace FlowBoard.Modules.Search.Infrastructure.ReadModels;

/// <summary>
/// Keyless read model over the <c>projects</c> table, owned by the Search module. The projects table
/// has no maintained search vector, so its document is computed on the fly in the query. Read-only.
/// </summary>
internal sealed class ProjectSearchDocument
{
    public Guid Id { get; init; }
    public Guid OrganisationId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string Status { get; init; } = null!;
    public DateTime? DeletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
