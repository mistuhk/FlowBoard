using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetAttachments;

/// <summary>Lists the active attachments on a task, oldest first.</summary>
/// <param name="TaskId">The task whose attachments to list.</param>
public sealed record GetAttachmentsQuery(Guid TaskId) : IQuery<Result<IReadOnlyList<AttachmentResponse>>>;
