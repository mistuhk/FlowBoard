namespace FlowBoard.Modules.Tasks.Application;

/// <summary>A cursor-paginated page of tasks.</summary>
/// <param name="Items">The tasks on this page, newest first.</param>
/// <param name="NextCursor">An opaque cursor for the next page, or <c>null</c> if this is the last page.</param>
public sealed record TaskPageResponse(IReadOnlyList<TaskResponse> Items, string? NextCursor);
