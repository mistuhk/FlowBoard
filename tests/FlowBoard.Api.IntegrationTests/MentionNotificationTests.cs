using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Outbox;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Notifications.Domain.Aggregates;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration test for the @mention flow: a comment mentioning a member's handle becomes a
/// user_mentioned notification once the outbox is dispatched, while an unknown handle is ignored.
/// </summary>
public sealed class MentionNotificationTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);
    private sealed record InvitationBody(Guid InvitationId, Guid OrganisationId, string InvitedEmail, string Role, DateTime ExpiresAt, string Token);

    private static HttpRequestMessage Rq(HttpMethod m, string uri, string token, object? body = null)
    {
        var r = new HttpRequestMessage(m, uri);
        r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) r.Content = JsonContent.Create(body);
        return r;
    }

    private async Task RunOutboxAsync()
    {
        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<OutboxProcessor>().RunAsync();
    }

    [Fact]
    public async Task Mentioning_a_member_notifies_them_and_an_unknown_handle_is_ignored()
    {
        // Owner sets up org/project/task.
        var owner = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, owner, "mention-owner@example.com");
        var (ownerTok, _) = await AuthTestHelpers.LoginAsync(owner, "mention-owner@example.com");

        var orgId = (await (await owner.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", ownerTok, new { name = "Mention Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projId = (await (await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", ownerTok, new { name = "Board" }))).Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks", ownerTok, new { title = "T", priority = "Medium" }))).Content.ReadFromJsonAsync<TaskBody>())!.Id;

        // Member bob joins (handle is the email local-part: "mention-bob").
        var inv = await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/invitations", ownerTok, new { email = "mention-bob@example.com", role = "Member" }));
        var token = (await inv.Content.ReadFromJsonAsync<InvitationBody>())!.Token;
        var bob = factory.CreateClient();
        var bobId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, bob, "mention-bob@example.com");
        var (bobTok, _) = await AuthTestHelpers.LoginAsync(bob, "mention-bob@example.com");
        (await bob.SendAsync(Rq(HttpMethod.Post, "/api/v1/invitations/accept", bobTok, new { token }))).StatusCode.Should().Be(HttpStatusCode.OK);

        // Owner mentions bob and a non-existent handle.
        var commentsPath = $"/api/v1/organisations/{orgId}/projects/{projId}/tasks/{taskId}/comments";
        (await owner.SendAsync(Rq(HttpMethod.Post, commentsPath, ownerTok, new { content = "@mention-bob and @ghost please review" })))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        await RunOutboxAsync();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mentions = await db.Set<Notification>()
            .Where(n => n.EntityId == taskId)
            .ToListAsync();

        // Exactly one mention notification: bob's. The unknown "@ghost" produced nothing.
        mentions.Should().ContainSingle();
        mentions[0].UserId.Should().Be(UserId.From(bobId));
        mentions[0].Type.Name.Should().Be("UserMentioned");
        mentions[0].IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task An_ambiguous_handle_notifies_nobody()
    {
        // Two members share the email local-part "amb" (different domains).
        var owner = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, owner, "ambig-owner@example.com");
        var (ownerTok, _) = await AuthTestHelpers.LoginAsync(owner, "ambig-owner@example.com");

        var orgId = (await (await owner.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", ownerTok, new { name = "Ambiguous Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projId = (await (await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", ownerTok, new { name = "Board" }))).Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks", ownerTok, new { title = "T", priority = "Medium" }))).Content.ReadFromJsonAsync<TaskBody>())!.Id;

        foreach (var email in new[] { "amb@one.example.com", "amb@two.example.com" })
        {
            var invite = await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/invitations", ownerTok, new { email, role = "Member" }));
            var token = (await invite.Content.ReadFromJsonAsync<InvitationBody>())!.Token;
            var client = factory.CreateClient();
            await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, email);
            var (tok, _) = await AuthTestHelpers.LoginAsync(client, email);
            (await client.SendAsync(Rq(HttpMethod.Post, "/api/v1/invitations/accept", tok, new { token }))).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var commentsPath = $"/api/v1/organisations/{orgId}/projects/{projId}/tasks/{taskId}/comments";
        (await owner.SendAsync(Rq(HttpMethod.Post, commentsPath, ownerTok, new { content = "@amb can you look?" })))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        await RunOutboxAsync();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mentions = await db.Set<Notification>().Where(n => n.EntityId == taskId).ToListAsync();

        mentions.Should().BeEmpty();
    }
}
