namespace FlowBoard.Infrastructure.Messaging;

/// <summary>SMTP transport settings, bound from the <c>Email</c> configuration section.</summary>
public sealed class EmailOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Email";

    /// <summary>The SMTP host (MailHog locally, a real relay in production).</summary>
    public string Host { get; init; } = "localhost";

    /// <summary>The SMTP port (1025 for MailHog).</summary>
    public int Port { get; init; } = 1025;

    /// <summary>Whether to negotiate TLS. Disabled for MailHog.</summary>
    public bool EnableSsl { get; init; }

    /// <summary>The From address applied to every message.</summary>
    public string From { get; init; } = "noreply@flowboard.local";
}
