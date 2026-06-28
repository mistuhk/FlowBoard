using FlowBoard.Infrastructure.Messaging;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Unit-level tests for the email transport wiring (no container needed): the queueing service
/// enqueues a Hangfire job rather than sending inline, and the job delegates to the SMTP sender.
/// </summary>
public sealed class EmailWiringTests
{
    [Fact]
    public void QueuedEmailService_enqueues_the_send_job_with_the_message()
    {
        var client = new Mock<IBackgroundJobClient>();
        var service = new QueuedEmailService(client.Object);

        service.SendAsync("ada@example.com", "Hello", "<p>Hi</p>");

        client.Verify(c => c.Create(
            It.Is<Job>(j =>
                j.Type == typeof(SendEmailNotificationJob)
                && j.Method.Name == nameof(SendEmailNotificationJob.SendAsync)
                && ((EmailMessage)j.Args[0]).To == "ada@example.com"
                && ((EmailMessage)j.Args[0]).Subject == "Hello"),
            It.IsAny<EnqueuedState>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailNotificationJob_delegates_to_the_sender()
    {
        var sender = new Mock<IEmailSender>();
        var job = new SendEmailNotificationJob(sender.Object);
        var message = new EmailMessage("ada@example.com", "Hello", "<p>Hi</p>");

        await job.SendAsync(message);

        sender.Verify(s => s.SendAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }
}
