namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Abstracts the EF Core DbContext transaction lifecycle.
/// <para>
/// The <c>TransactionBehaviour</c> calls these methods around every command handler:
/// <list type="number">
///   <item><see cref="BeginTransactionAsync"/> — opens an explicit DB transaction.</item>
///   <item>Handler executes and modifies aggregates via repositories.</item>
///   <item><see cref="SaveChangesAsync"/> — serialises domain events to the outbox
///         table and flushes all EF Core tracked changes within the open transaction.</item>
///   <item><see cref="CommitTransactionAsync"/> — commits the transaction atomically.</item>
/// </list>
/// On any exception, <see cref="RollbackTransactionAsync"/> is called instead of commit.
/// </para>
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Opens an explicit database transaction. Also sets the Postgres
    /// <c>SET LOCAL app.current_organisation_id</c> session variable if the
    /// tenant context has been resolved, enabling Row-Level Security policies.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Serialises all domain events raised by tracked aggregate roots to the
    /// <c>outbox_messages</c> table, then flushes all pending EF Core changes.
    /// Both writes occur within the same open transaction.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the current transaction.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the current transaction on failure.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
