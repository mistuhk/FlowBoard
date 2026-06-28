using Hangfire;

namespace FlowBoard.Infrastructure.Messaging;

/// <summary>
/// Hangfire job that delivers a queued email. Retried up to three times with increasing backoff; a
/// message that still fails is moved to the failed set rather than lost. Resolved from the container
/// so the SMTP sender is injected.
/// </summary>
public sealed class SendEmailNotificationJob(IEmailSender sender)
{
    /// <summary>Sends the queued message over SMTP.</summary>
    /// <param name="message">The message to deliver.</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [10, 60, 300])]
    public Task SendAsync(EmailMessage message) => sender.SendAsync(message);
}
