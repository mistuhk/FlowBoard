using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.ActivityLog.Application.Queries.GetUserActivity;

/// <summary>Lists the current user's activity within the organisation (newest first), cursor-paginated.</summary>
/// <param name="Cursor">An opaque cursor from a previous page, or <c>null</c> for the first page.</param>
/// <param name="Limit">The maximum page size (clamped server-side).</param>
public sealed record GetUserActivityQuery(string? Cursor, int? Limit) : IQuery<ActivityPageResponse>;
