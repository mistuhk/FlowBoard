using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Infrastructure.Outbox;
using FlowBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;

namespace FlowBoard.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>.
/// <para>
/// Wraps each command handler in an explicit Postgres transaction so that:
/// <list type="bullet">
///   <item>Aggregate changes and outbox messages are written atomically.</item>
///   <item><c>SET LOCAL app.current_organisation_id</c> is issued inside the
///         transaction boundary, enabling Postgres Row-Level Security.</item>
/// </list>
/// </para>
/// </summary>
public sealed class UnitOfWork(AppDbContext context, ITenantContext tenantContext)
    : IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    private static readonly JsonSerializerOptions SerialiserOptions = new()
    {
        WriteIndented = false
    };

    /// <inheritdoc/>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await context.Database
            .BeginTransactionAsync(cancellationToken);

        // Set the Postgres session variable for Row-Level Security. set_config(..., is_local => true)
        // is the transaction-scoped equivalent of SET LOCAL and, unlike SET LOCAL, accepts a bind
        // parameter for the value, so the organisation id is passed safely as a parameter.
        if (tenantContext.IsResolved)
        {
            var orgId = tenantContext.CurrentOrganisationId.Value.ToString();
            await context.Database.ExecuteSqlRawAsync(
                "SELECT set_config('app.current_organisation_id', {0}, true)", orgId);
        }
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect all domain events from tracked aggregate roots
        var aggregateRoots = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Serialise each event to an outbox record (same transaction - guaranteed delivery)
        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                AggregateType = domainEvent.GetType().DeclaringType?.Name
                    ?? domainEvent.GetType().Name,
                AggregateId = Guid.Empty,
                EventType = domainEvent.GetType().FullName!,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(),
                    SerialiserOptions),
                CreatedAt = DateTime.UtcNow
            };

            await context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        }

        // Flush aggregate changes + outbox messages in one round-trip
        var result = await context.SaveChangesAsync(cancellationToken);

        // Clear events so they are not re-processed on a second SaveChanges call
        foreach (var aggregate in aggregateRoots)
            aggregate.ClearDomainEvents();

        return result;
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException(
                "CommitTransactionAsync called without an active transaction. " +
                "Ensure BeginTransactionAsync was called first.");

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null) return;

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
}
