using FlowBoard.Application.Abstractions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Test double for <see cref="IEmailService"/>. The integration environment has no SMTP server, so
/// email sends are swallowed (the real implementation enqueues a Hangfire job that would hit MailHog).
/// </summary>
internal sealed class FakeEmailService : IEmailService
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
