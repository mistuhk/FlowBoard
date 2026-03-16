namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Abstraction for sending transactional emails.
/// Implemented in the Infrastructure layer (e.g. SMTP, SendGrid).
/// Email delivery is always performed asynchronously via a Hangfire background job —
/// never inline within a command handler.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a transactional email to the specified recipient.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML body of the email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
