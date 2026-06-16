using FlowBoard.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Infrastructure.Messaging;

/// <summary>
/// Placeholder <see cref="IEmailService"/> that logs instead of sending. It exists so
/// handlers depending on <see cref="IEmailService"/> can be constructed before the real
/// transport is built. Replaced by the MailKit/SMTP implementation in Sprint 6.
/// </summary>
internal sealed class NoOpEmailService(ILogger<NoOpEmailService> logger) : IEmailService
{
    /// <inheritdoc/>
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Email delivery is not yet configured; dropping message to {Recipient} with subject {Subject}.",
            to, subject);
        return Task.CompletedTask;
    }
}
