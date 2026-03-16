namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Abstracts the EF Core DbContext <c>SaveChangesAsync</c> call, combined with
/// serialisation of domain events to the outbox table.
/// The <c>TransactionBehaviour</c> calls this at the end of every command handler.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes and serialises any raised domain events
    /// to the <c>outbox_messages</c> table within the same database transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
