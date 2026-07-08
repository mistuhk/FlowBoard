using NpgsqlTypes;

namespace FlowBoard.Modules.Search.Infrastructure.ReadModels;

/// <summary>
/// Keyless read model over the <c>tasks</c> table, owned by the Search module so it can query tasks
/// without depending on the Tasks module's aggregate. Read-only; never written through EF.
/// </summary>
internal sealed class TaskSearchDocument
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid OrganisationId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string Status { get; init; } = null!;
    public string Priority { get; init; } = null!;
    public Guid? AssigneeId { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? DeletedAt { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>The maintained full-text search vector (a generated column on <c>tasks</c>).</summary>
    public NpgsqlTsVector SearchVector { get; init; } = null!;
}
