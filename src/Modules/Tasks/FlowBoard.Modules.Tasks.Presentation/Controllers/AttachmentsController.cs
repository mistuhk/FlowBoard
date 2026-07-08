using FlowBoard.Application.Authorization;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Tasks.Application;
using FlowBoard.Modules.Tasks.Application.Commands.ConfirmAttachmentUpload;
using FlowBoard.Modules.Tasks.Application.Commands.RemoveAttachment;
using FlowBoard.Modules.Tasks.Application.Commands.RequestAttachmentUpload;
using FlowBoard.Modules.Tasks.Application.Queries.GetAttachmentDownloadUrl;
using FlowBoard.Modules.Tasks.Application.Queries.GetAttachments;
using FlowBoard.Modules.Tasks.Presentation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Modules.Tasks.Presentation.Controllers;

/// <summary>
/// Task attachment endpoints under
/// <c>/api/v1/organisations/{orgId}/projects/{projectId}/tasks/{taskId}/attachments</c>. The API only
/// coordinates pre-signed URLs; file bytes are uploaded and downloaded directly to storage. Requires
/// organisation membership; removal is restricted (uploader or Admin/Owner) by the handler.
/// </summary>
[ApiController]
[Authorize(Policy = OrganisationPolicies.Member)]
[Route("api/v1/organisations/{orgId:guid}/projects/{projectId:guid}/tasks/{taskId:guid}/attachments")]
public sealed class AttachmentsController(ISender sender) : ControllerBase
{
    /// <summary>Lists the task's active attachments.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AttachmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAttachmentsQuery(taskId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    /// <summary>Requests a pre-signed URL to upload a file directly to storage.</summary>
    [HttpPost("upload-url")]
    [ProducesResponseType(typeof(UploadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RequestUpload(Guid taskId, [FromBody] RequestAttachmentUploadRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RequestAttachmentUploadCommand(taskId, request.FileName, request.FileSizeBytes, request.MimeType), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    /// <summary>Confirms a completed upload and records the attachment.</summary>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Confirm(Guid taskId, [FromBody] ConfirmAttachmentUploadRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ConfirmAttachmentUploadCommand(taskId, request.StorageKey, request.FileName, request.FileSizeBytes, request.MimeType), cancellationToken);
        return result.IsSuccess
            ? Created($"{Request.Path}/{result.Value.Id}", result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Returns a pre-signed download URL for an attachment.</summary>
    [HttpGet("{attachmentId:guid}/download-url")]
    [ProducesResponseType(typeof(DownloadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadUrl(Guid taskId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAttachmentDownloadUrlQuery(taskId, attachmentId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    /// <summary>Soft-deletes an attachment. Uploader or Admin/Owner only.</summary>
    [HttpDelete("{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid taskId, Guid attachmentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveAttachmentCommand(taskId, attachmentId), cancellationToken);
        return result.IsSuccess ? NoContent() : ToProblem(result.Error);
    }

    private ObjectResult ToProblem(Error error)
    {
        var (status, title, type) = error.Code switch
        {
            "Tasks.InvalidStorageKey" or "Tasks.AttachmentNotUploaded" =>
                (StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity", "attachment-upload-invalid"),
            "Tasks.AttachmentNotFound" =>
                (StatusCodes.Status404NotFound, "Not Found", "attachment-not-found"),
            _ => (StatusCodes.Status404NotFound, "Not Found", "task-not-found"),
        };

        return new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://flowboard.io/errors/{type}",
            Detail = error.Description,
            Extensions = { ["traceId"] = HttpContext.TraceIdentifier, ["code"] = error.Code },
        })
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
    }
}
