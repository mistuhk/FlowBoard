using FluentValidation;

namespace FlowBoard.Modules.Notifications.Application.Commands.MarkAllAsRead;

/// <summary>
/// Validates <see cref="MarkAllAsReadCommand"/>. The command carries no input (it acts on the current
/// user), so there are no rules; the validator exists for consistency with the pipeline.
/// </summary>
public sealed class MarkAllAsReadCommandValidator : AbstractValidator<MarkAllAsReadCommand>
{
    /// <summary>Configures the (empty) validation rules.</summary>
    public MarkAllAsReadCommandValidator()
    {
    }
}
