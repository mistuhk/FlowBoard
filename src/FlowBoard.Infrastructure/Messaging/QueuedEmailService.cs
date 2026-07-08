using FlowBoard.Application.Abstractions;
using Hangfire;

namespace FlowBoard.Infrastructure.Messaging;

/// <summary>
/// <see cref="IEmailService"/> implementation that enqueues a Hangfire <see cref="SendEmailNotificationJob"/>
/// rather than sending inline. Callers (event handlers) therefore never block on SMTP, and delivery
/// gets its own retry policy independent of the work that triggered it.
/// </summary>
internal sealed class QueuedEmailService(IBackgroundJobClient jobs) : IEmailService
{
    /// <inheritdoc/>
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        jobs.Enqueue<SendEmailNotificationJob>(job => job.SendAsync(new EmailMessage(to, subject, htmlBody)));
        return Task.CompletedTask;
    }
}
