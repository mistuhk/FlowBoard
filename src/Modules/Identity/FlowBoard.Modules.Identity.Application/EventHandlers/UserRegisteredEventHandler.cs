using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Modules.Identity.Application.EventHandlers;

/// <summary>
/// Reacts to <see cref="UserRegisteredEvent"/> (dispatched from the outbox) by sending the
/// account's email-verification message.
/// <para>
/// Delivery is delegated to <see cref="IEmailService"/>, whose Infrastructure implementation
/// performs the actual send on a Hangfire background job, the Application layer never
/// references Hangfire directly, preserving the dependency rule.
/// </para>
/// </summary>
public sealed class UserRegisteredEventHandler(
    IEmailService emailService,
    ICacheService cache,
    ILogger<UserRegisteredEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserRegisteredEvent>>
{
    /// <inheritdoc/>
    public async Task Handle(
        DomainEventNotification<UserRegisteredEvent> notification,
        CancellationToken cancellationToken)
    {
        var @event = notification.DomainEvent;

        var token = await cache.GetAsync<string>(
            EmailVerificationTokens.PendingKey(@event.UserId),
            cancellationToken);

        if (token is null)
        {
            // The token has expired or was already consumed, nothing to send.
            logger.LogWarning(
                "No pending email-verification token for user {UserId}; verification email skipped.",
                @event.UserId);
            return;
        }

        var verificationLink = $"/verify-email?token={token}";
        var body =
            $"""
             <p>Welcome to FlowBoard.</p>
             <p>Please confirm your email address by visiting the link below:</p>
             <p><a href="{verificationLink}">{verificationLink}</a></p>
             <p>This link expires in 24 hours.</p>
             """;

        await emailService.SendAsync(
            @event.Email,
            "Verify your FlowBoard email address",
            body,
            cancellationToken);
    }
}
