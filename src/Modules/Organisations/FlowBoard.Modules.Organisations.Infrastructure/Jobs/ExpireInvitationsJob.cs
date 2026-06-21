using FlowBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Modules.Organisations.Infrastructure.Jobs;

/// <summary>
/// Hourly Hangfire job that purges expired, unaccepted invitations. Expiry is a computed state
/// (no stored flag): an invitation is expired once <c>accepted_at IS NULL</c> and
/// <c>expires_at &lt; now()</c>. Accepted invitations are retained as a record and are never removed
/// here. This is a maintenance operation, so it deletes in bulk rather than through the aggregate.
/// </summary>
public sealed class ExpireInvitationsJob(AppDbContext context, ILogger<ExpireInvitationsJob> logger)
{
    /// <summary>Deletes all expired, unaccepted invitations.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var removed = await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM invitations WHERE accepted_at IS NULL AND expires_at < now()",
            cancellationToken);

        if (removed > 0)
            logger.LogInformation("Expired and removed {Count} pending invitation(s).", removed);
    }
}
