using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for the attachment pre-signed URL flow: request an upload URL, confirm the
/// upload, list, get a download URL, and remove. Uses the fake storage service.
/// </summary>
[Collection("Integration")]
public sealed class AttachmentEndpointTests(FlowBoardApiFactory factory)
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);
    private sealed record UploadUrlBody(string StorageKey, string UploadUrl, DateTime ExpiresAtUtc);
    private sealed record AttachmentBody(Guid Id, Guid TaskId, Guid UploadedById, string FileName, long FileSizeBytes, string MimeType, DateTime CreatedAt);
    private sealed record DownloadUrlBody(string Url, DateTime ExpiresAtUtc);

    private static HttpRequestMessage Rq(HttpMethod m, string uri, string token, object? body = null)
    {
        var r = new HttpRequestMessage(m, uri);
        r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) r.Content = JsonContent.Create(body);
        return r;
    }

    [Fact]
    public async Task Attachment_upload_flow_request_confirm_list_download_remove()
    {
        var client = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "attach-owner@example.com");
        var (token, _) = await AuthTestHelpers.LoginAsync(client, "attach-owner@example.com");

        var orgId = (await (await client.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", token, new { name = "Attach Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Board" }))).Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks", token, new { title = "T", priority = "Medium" }))).Content.ReadFromJsonAsync<TaskBody>())!.Id;
        var basePath = $"/api/v1/organisations/{orgId}/projects/{projId}/tasks/{taskId}/attachments";

        // Request an upload URL.
        var requested = await client.SendAsync(Rq(HttpMethod.Post, $"{basePath}/upload-url", token,
            new { fileName = "spec.pdf", fileSizeBytes = 2048, mimeType = "application/pdf" }));
        requested.StatusCode.Should().Be(HttpStatusCode.OK);
        var upload = (await requested.Content.ReadFromJsonAsync<UploadUrlBody>())!;
        upload.StorageKey.Should().StartWith($"{orgId}/{taskId}/");
        upload.UploadUrl.Should().NotBeNullOrEmpty();

        // A storage key for a different task is rejected.
        var badConfirm = await client.SendAsync(Rq(HttpMethod.Post, $"{basePath}/confirm", token,
            new { storageKey = $"{orgId}/{Guid.NewGuid()}/x/spec.pdf", fileName = "spec.pdf", fileSizeBytes = 2048, mimeType = "application/pdf" }));
        badConfirm.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // An oversized request is rejected by validation.
        var tooBig = await client.SendAsync(Rq(HttpMethod.Post, $"{basePath}/upload-url", token,
            new { fileName = "big.pdf", fileSizeBytes = 26L * 1024 * 1024, mimeType = "application/pdf" }));
        tooBig.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Confirm the upload.
        var confirmed = await client.SendAsync(Rq(HttpMethod.Post, $"{basePath}/confirm", token,
            new { storageKey = upload.StorageKey, fileName = "spec.pdf", fileSizeBytes = 2048, mimeType = "application/pdf" }));
        confirmed.StatusCode.Should().Be(HttpStatusCode.Created);
        var attachment = (await confirmed.Content.ReadFromJsonAsync<AttachmentBody>())!;
        attachment.FileName.Should().Be("spec.pdf");

        // List shows it.
        var list = await (await client.SendAsync(Rq(HttpMethod.Get, basePath, token))).Content.ReadFromJsonAsync<List<AttachmentBody>>();
        list!.Should().ContainSingle(a => a.Id == attachment.Id);

        // Download URL.
        var download = await client.SendAsync(Rq(HttpMethod.Get, $"{basePath}/{attachment.Id}/download-url", token));
        download.StatusCode.Should().Be(HttpStatusCode.OK);
        (await download.Content.ReadFromJsonAsync<DownloadUrlBody>())!.Url.Should().NotBeNullOrEmpty();

        // Remove, then the list is empty.
        (await client.SendAsync(Rq(HttpMethod.Delete, $"{basePath}/{attachment.Id}", token))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var after = await (await client.SendAsync(Rq(HttpMethod.Get, basePath, token))).Content.ReadFromJsonAsync<List<AttachmentBody>>();
        after!.Should().BeEmpty();
    }
}
