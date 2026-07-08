using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetAttachments;

/// <summary>
/// Handles <see cref="GetAttachmentsQuery"/>: returns the task's active attachments, oldest first.
/// Read-only. Soft-deleted attachments are excluded in memory.
/// </summary>
public sealed class GetAttachmentsQueryHandler(
    ITenantContext tenantContext,
    ITaskRepository tasks)
    : IRequestHandler<GetAttachmentsQuery, Result<IReadOnlyList<AttachmentResponse>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<AttachmentResponse>>> Handle(GetAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(
            TaskId.From(request.TaskId), tenantContext.CurrentOrganisationId, cancellationToken);

        if (task is null)
            return Result.Failure<IReadOnlyList<AttachmentResponse>>(TaskErrors.NotFound);

        IReadOnlyList<AttachmentResponse> attachments = task.Attachments
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.CreatedAt)
            .Select(AttachmentResponse.From)
            .ToList();

        return Result.Success(attachments);
    }
}
