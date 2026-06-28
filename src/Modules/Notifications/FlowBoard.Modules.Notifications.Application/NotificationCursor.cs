using System.Text;

namespace FlowBoard.Modules.Notifications.Application;

/// <summary>
/// Opaque, base64-encoded pagination cursor for the notification list. Encodes the created-at of the
/// last notification on a page (the list is ordered newest first), so the next page continues from there.
/// </summary>
public static class NotificationCursor
{
    /// <summary>Encodes a created-at timestamp into an opaque cursor string.</summary>
    /// <param name="createdAt">The created-at of the last notification on the page.</param>
    public static string Encode(DateTime createdAt) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(createdAt.Ticks.ToString()))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    /// <summary>
    /// Decodes an opaque cursor back into a created-at timestamp. Returns <c>null</c> for a null,
    /// empty, or malformed cursor (treated as "from the start").
    /// </summary>
    /// <param name="cursor">The cursor string, or <c>null</c>.</param>
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
