using FlowBoard.Application.Abstractions;
using FlowBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Modules.Tasks.Infrastructure.Jobs;

/// <summary>
/// Daily Hangfire job that permanently removes attachments soft-deleted more than the retention period
/// ago, from both object storage and the database. A maintenance operation, so it works in bulk
/// rather than through the aggregate. If a storage delete fails the row is left for the next run.
/// </summary>
public sealed class HardDeleteAttachmentsJob(
    AppDbContext context,
    IStorageService storage,
    ILogger<HardDeleteAttachmentsJob> logger)
{
    private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    private sealed record AttachmentRow(Guid Id, string StorageKey);

    /// <summary>Purges expired soft-deleted attachments.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow - Retention;

        var expired = await context.Database
            .SqlQuery<AttachmentRow>(
                $"""
                 SELECT id AS "Id", storage_key AS "StorageKey"
                 FROM file_attachments
                 WHERE deleted_at IS NOT NULL AND deleted_at < {cutoff}
                 """)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
            return;

        var purged = 0;
        foreach (var row in expired)
        {
            try
            {
                await storage.DeleteAsync(row.StorageKey, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception,
                    "Failed to delete storage object {StorageKey}; leaving the row for the next run.", row.StorageKey);
                continue;
            }

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM file_attachments WHERE id = {row.Id}", cancellationToken);
            purged++;
        }

        logger.LogInformation("Hard-deleted {Count} attachment(s) past retention.", purged);
    }
}
