using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Organisations.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Modules.Organisations.Application.EventHandlers;

/// <summary>
/// Reacts to <see cref="MemberInvitedEvent"/> (dispatched from the outbox) by sending the
/// invitation email.
/// <para>
/// The raw token is read from the cache (stashed when the invitation was issued); only the hash is
/// persisted. Delivery is delegated to <see cref="IEmailService"/>, whose Infrastructure
/// implementation sends on a Hangfire background job, so the Application layer never references
/// Hangfire directly and the dependency rule is preserved.
/// </para>
/// </summary>
public sealed class MemberInvitedEventHandler(
    IEmailService emailService,
    ICacheService cache,
    ILogger<MemberInvitedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<MemberInvitedEvent>>
{
    /// <inheritdoc/>
    public async Task Handle(
        DomainEventNotification<MemberInvitedEvent> notification,
        CancellationToken cancellationToken)
    {
        var @event = notification.DomainEvent;

        var token = await cache.GetAsync<string>(
            InvitationTokens.PendingKey(@event.OrganisationId, @event.InvitedEmail),
            cancellationToken);

        if (token is null)
        {
            // The token has expired or was already consumed, nothing to send.
            logger.LogWarning(
                "No pending invitation token for {Email} in organisation {OrganisationId}; invitation email skipped.",
                @event.InvitedEmail,
                @event.OrganisationId);
            return;
        }

        var acceptLink = $"/invitations/accept?token={token}";
        var body =
            $"""
             <p>You have been invited to join an organisation on FlowBoard as a {@event.Role}.</p>
             <p>To accept the invitation, visit the link below:</p>
             <p><a href="{acceptLink}">{acceptLink}</a></p>
             <p>This invitation expires in 48 hours.</p>
             """;

        await emailService.SendAsync(
            @event.InvitedEmail,
            "You have been invited to FlowBoard",
            body,
            cancellationToken);
    }
}
