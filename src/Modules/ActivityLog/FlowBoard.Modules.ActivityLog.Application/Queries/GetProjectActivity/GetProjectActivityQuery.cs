using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.ActivityLog.Application.Queries.GetProjectActivity;

/// <summary>Lists project-level activity (newest first), cursor-paginated.</summary>
/// <param name="ProjectId">The project whose activity to list.</param>
/// <param name="Cursor">An opaque cursor from a previous page, or <c>null</c> for the first page.</param>
/// <param name="Limit">The maximum page size (clamped server-side).</param>
public sealed record GetProjectActivityQuery(Guid ProjectId, string? Cursor, int? Limit) : IQuery<ActivityPageResponse>;
