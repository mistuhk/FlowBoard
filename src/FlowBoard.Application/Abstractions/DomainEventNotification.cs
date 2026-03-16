using FlowBoard.Domain.Primitives;
using MediatR;

namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Adapts a domain event into a MediatR <see cref="INotification"/> so it can be
/// dispatched via <see cref="IPublisher"/>.
/// <para>
/// This adapter lives in the Application layer, keeping the Domain layer free of any
/// MediatR dependency. The <c>OutboxProcessor</c> (Infrastructure) deserialises each
/// <c>outbox_messages</c> record, wraps it in this type, and publishes it so all
/// registered <c>INotificationHandler</c> implementations receive it.
/// </para>
/// </summary>
/// <typeparam name="TEvent">The concrete domain event type.</typeparam>
/// <param name="DomainEvent">The domain event instance being wrapped.</param>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent)
    : INotification where TEvent : IDomainEvent;
