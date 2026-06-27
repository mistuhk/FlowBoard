using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for task comments: add/list/edit/delete lifecycle and the author-or-Admin/Owner
/// edit authorisation (a plain member editing another's comment is forbidden).
/// </summary>
public sealed class CommentsEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);
    private sealed record CommentBody(Guid Id, Guid TaskId, Guid AuthorId, string Content, List<string> Mentions, DateTime CreatedAt, DateTime UpdatedAt);
    private sealed record InvitationBody(Guid InvitationId, Guid OrganisationId, string InvitedEmail, string Role, DateTime ExpiresAt, string Token);

    private static HttpRequestMessage Rq(HttpMethod m, string uri, string token, object? body = null)
    {
        var r = new HttpRequestMessage(m, uri);
        r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) r.Content = JsonContent.Create(body);
        return r;
    }

    [Fact]
    public async Task Comment_lifecycle_and_edit_authorisation()
    {
        // Owner sets up org/project/task.
        var owner = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, owner, "comments-owner@example.com");
        var (ownerTok, _) = await AuthTestHelpers.LoginAsync(owner, "comments-owner@example.com");

        var orgId = (await (await owner.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", ownerTok, new { name = "Comments Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projId = (await (await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", ownerTok, new { name = "Board" }))).Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks", ownerTok, new { title = "T", priority = "Medium" }))).Content.ReadFromJsonAsync<TaskBody>())!.Id;
        var commentsPath = $"/api/v1/organisations/{orgId}/projects/{projId}/tasks/{taskId}/comments";

        // Add
        var add = await owner.SendAsync(Rq(HttpMethod.Post, commentsPath, ownerTok, new { content = "First comment" }));
        add.StatusCode.Should().Be(HttpStatusCode.Created);
        var comment = (await add.Content.ReadFromJsonAsync<CommentBody>())!;
        comment.Content.Should().Be("First comment");

        // List
        var list = await (await owner.SendAsync(Rq(HttpMethod.Get, commentsPath, ownerTok))).Content.ReadFromJsonAsync<List<CommentBody>>();
        list!.Should().ContainSingle(c => c.Id == comment.Id);

        // Edit by author
        var edit = await owner.SendAsync(Rq(HttpMethod.Put, $"{commentsPath}/{comment.Id}", ownerTok, new { content = "Edited" }));
        edit.StatusCode.Should().Be(HttpStatusCode.OK);
        (await edit.Content.ReadFromJsonAsync<CommentBody>())!.Content.Should().Be("Edited");

        // A plain member cannot edit the owner's comment.
        var inv = await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/invitations", ownerTok, new { email = "comments-member@example.com", role = "Member" }));
        var token = (await inv.Content.ReadFromJsonAsync<InvitationBody>())!.Token;
        var member = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, member, "comments-member@example.com");
        var (memberTok, _) = await AuthTestHelpers.LoginAsync(member, "comments-member@example.com");
        (await member.SendAsync(Rq(HttpMethod.Post, "/api/v1/invitations/accept", memberTok, new { token }))).StatusCode.Should().Be(HttpStatusCode.OK);

        var forbidden = await member.SendAsync(Rq(HttpMethod.Put, $"{commentsPath}/{comment.Id}", memberTok, new { content = "hijack" }));
        forbidden.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Owner deletes; list is then empty.
        (await owner.SendAsync(Rq(HttpMethod.Delete, $"{commentsPath}/{comment.Id}", ownerTok))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var after = await (await owner.SendAsync(Rq(HttpMethod.Get, commentsPath, ownerTok))).Content.ReadFromJsonAsync<List<CommentBody>>();
        after!.Should().BeEmpty();
    }
}
