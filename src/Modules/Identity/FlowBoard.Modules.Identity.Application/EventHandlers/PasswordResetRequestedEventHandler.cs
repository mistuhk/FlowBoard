using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Modules.Identity.Application.EventHandlers;

/// <summary>
/// Reacts to <see cref="PasswordResetRequestedEvent"/> (dispatched from the outbox) by sending the
/// password-reset email.
/// <para>
/// Delivery is delegated to <see cref="IEmailService"/>, whose Infrastructure implementation
/// performs the actual send on a Hangfire background job, so the Application layer never references
/// Hangfire directly and the dependency rule is preserved.
/// </para>
/// </summary>
public sealed class PasswordResetRequestedEventHandler(
    IEmailService emailService,
    ICacheService cache,
    ILogger<PasswordResetRequestedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<PasswordResetRequestedEvent>>
{
    /// <inheritdoc/>
    public async Task Handle(
        DomainEventNotification<PasswordResetRequestedEvent> notification,
        CancellationToken cancellationToken)
    {
        var @event = notification.DomainEvent;

        var token = await cache.GetAsync<string>(
            PasswordResetTokens.PendingKey(@event.UserId),
            cancellationToken);

        if (token is null)
        {
            // The token has expired or was already consumed, nothing to send.
            logger.LogWarning(
                "No pending password-reset token for user {UserId}; reset email skipped.",
                @event.UserId);
            return;
        }

        var resetLink = $"/reset-password?token={token}";
        var body =
            $"""
             <p>We received a request to reset your FlowBoard password.</p>
             <p>If this was you, set a new password using the link below:</p>
             <p><a href="{resetLink}">{resetLink}</a></p>
             <p>This link expires in one hour. If you did not request a reset, you can ignore this email.</p>
             """;

        await emailService.SendAsync(
            @event.Email,
            "Reset your FlowBoard password",
            body,
            cancellationToken);
    }
}
