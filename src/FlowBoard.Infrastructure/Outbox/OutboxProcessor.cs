using System.Collections.Concurrent;
using System.Text.Json;
using FlowBoard.Application.Abstractions;
using FlowBoard.Infrastructure.Persistence;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Infrastructure.Outbox;

/// <summary>
/// Dispatches the transactional outbox. Reads unprocessed <see cref="OutboxMessage"/> records,
/// rehydrates each domain event, wraps it in a <see cref="DomainEventNotification{TEvent}"/>, and
/// publishes it via MediatR so handlers in any module react. Successful records are stamped with
/// <see cref="OutboxMessage.ProcessedAt"/>; failures record <see cref="OutboxMessage.Error"/> and
/// are retried on a later run. Handlers must be idempotent, since a message may be redelivered.
/// </summary>
public sealed class OutboxProcessor(
    AppDbContext context,
    IPublisher publisher,
    ILogger<OutboxProcessor> logger)
{
    private const int BatchSize = 100;

    private static readonly JsonSerializerOptions SerialiserOptions = new() { WriteIndented = false };
    private static readonly ConcurrentDictionary<string, Type?> TypeCache = new();

    /// <summary>
    /// Processes a batch of unprocessed outbox messages, oldest first. Decorated so Hangfire never
    /// runs two dispatch passes concurrently (a redelivery is still possible across passes, hence
    /// the idempotency requirement on handlers).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var messages = await context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            try
            {
                await DispatchAsync(message, cancellationToken);
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception exception)
            {
                // Leave ProcessedAt null so the message is retried; record the latest error.
                logger.LogError(
                    exception,
                    "Failed to dispatch outbox message {OutboxId} of type {EventType}.",
                    message.Id,
                    message.EventType);
                message.Error = exception.Message;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var eventType = ResolveEventType(message.EventType)
            ?? throw new InvalidOperationException($"Unknown domain event type '{message.EventType}'.");

        var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, SerialiserOptions)
            ?? throw new InvalidOperationException($"Outbox message {message.Id} deserialised to null.");

        var notification = Activator.CreateInstance(
            typeof(DomainEventNotification<>).MakeGenericType(eventType), domainEvent)!;

        await publisher.Publish(notification, cancellationToken);
    }

    // Events are stored by full type name (no assembly), so resolve across the loaded FlowBoard
    // assemblies and cache the result.
    private static Type? ResolveEventType(string fullName) =>
        TypeCache.GetOrAdd(fullName, name =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.FullName?.StartsWith("FlowBoard", StringComparison.Ordinal) == true)
                .Select(assembly => assembly.GetType(name))
                .FirstOrDefault(type => type is not null));
}
