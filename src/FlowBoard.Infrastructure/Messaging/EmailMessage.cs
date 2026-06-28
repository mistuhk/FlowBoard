namespace FlowBoard.Infrastructure.Messaging;

/// <summary>
/// A transactional email queued for delivery. Passed as the argument to the Hangfire
/// <see cref="SendEmailNotificationJob"/>, so it must be serialisable.
/// </summary>
/// <param name="To">The recipient address.</param>
/// <param name="Subject">The subject line.</param>
/// <param name="HtmlBody">The message body (HTML).</param>
public sealed record EmailMessage(string To, string Subject, string HtmlBody);

/// <summary>Sends a single email over SMTP. Implemented with MailKit.</summary>
public interface IEmailSender
{
    /// <summary>Delivers the message over SMTP.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
