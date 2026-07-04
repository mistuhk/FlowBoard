using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Commands.RemoveAttachment;

/// <summary>Soft-deletes an attachment. Permitted for the uploader, or an Admin or the Owner.</summary>
/// <param name="TaskId">The task the attachment belongs to.</param>
/// <param name="AttachmentId">The attachment to remove.</param>
public sealed record RemoveAttachmentCommand(Guid TaskId, Guid AttachmentId) : ICommand<Result>;
