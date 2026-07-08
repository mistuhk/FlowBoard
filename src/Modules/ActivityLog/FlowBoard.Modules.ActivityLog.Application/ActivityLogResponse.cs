using System.Text;
using FlowBoard.Modules.ActivityLog.Domain;

namespace FlowBoard.Modules.ActivityLog.Application;

/// <summary>The public representation of an activity-log entry.</summary>
/// <param name="Id">The entry id.</param>
/// <param name="EntityType">The kind of entity acted upon.</param>
/// <param name="EntityId">The id of the entity acted upon.</param>
/// <param name="EventType">The event token.</param>
/// <param name="ActorId">The acting user, or <c>null</c> if system-generated.</param>
/// <param name="Metadata">Additional context.</param>
/// <param name="CreatedAt">When the activity occurred.</param>
public sealed record ActivityLogResponse(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string EventType,
    Guid? ActorId,
    IReadOnlyDictionary<string, object>? Metadata,
    DateTime CreatedAt)
{
    /// <summary>Maps an <see cref="ActivityLogEntry"/> to its response representation.</summary>
    public static ActivityLogResponse From(ActivityLogEntry entry) =>
        new(entry.Id, entry.EntityType, entry.EntityId, entry.EventType, entry.ActorId?.Value, entry.Metadata, entry.CreatedAt);
}

/// <summary>A cursor-paginated page of activity-log entries.</summary>
/// <param name="Items">The entries on this page, newest first.</param>
/// <param name="NextCursor">An opaque cursor for the next page, or <c>null</c> if this is the last page.</param>
public sealed record ActivityPageResponse(IReadOnlyList<ActivityLogResponse> Items, string? NextCursor);

/// <summary>
/// Opaque, base64-encoded pagination cursor for activity feeds. Encodes the created-at of the last
/// entry on a page (newest first), so the next page continues from there.
/// </summary>
public static class ActivityCursor
{
    /// <summary>Encodes a created-at timestamp into an opaque cursor string.</summary>
    public static string Encode(DateTime createdAt) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(createdAt.Ticks.ToString()))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    /// <summary>Decodes an opaque cursor back into a created-at timestamp, or <c>null</c> if malformed.</summary>
    public static DateTime? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var padded = cursor.Replace('-', '+').Replace('_', '/');
            padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
            var ticks = long.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(padded)));
            return new DateTime(ticks, DateTimeKind.Utc);
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or ArgumentException)
        {
            return null;
        }
    }
}
