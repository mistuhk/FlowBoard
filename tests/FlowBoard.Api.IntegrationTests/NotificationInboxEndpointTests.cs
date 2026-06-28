using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlowBoard.Infrastructure.Outbox;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for the in-app notification inbox: list, unread-count, mark-read, mark-all-read,
/// and cross-user isolation (one user can neither see nor mark another user's notifications).
/// </summary>
public sealed class NotificationInboxEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);
    private sealed record NotifItem(Guid Id, string Type, string Message, string? EntityType, Guid? EntityId, bool IsRead, Guid OrganisationId, DateTime CreatedAt);
    private sealed record NotifPage(List<NotifItem> Items, string? NextCursor);
    private sealed record UnreadResp(int Count);

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

    private async Task<(string token, Guid userId, Guid orgId, Guid projectId, Guid taskId)> SetupWithTaskAsync(string email)
    {
        var client = factory.CreateClient();
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, email);
        var (token, _) = await AuthTestHelpers.LoginAsync(client, email);
        var orgId = (await (await client.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", token, new { name = $"Inbox {email}" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Board" }))).Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks", token, new { title = "T", priority = "Medium" }))).Content.ReadFromJsonAsync<TaskBody>())!.Id;
        return (token, userId, orgId, projId, taskId);
    }

    [Fact]
    public async Task Inbox_supports_list_unread_count_mark_read_and_mark_all_read()
    {
        var (token, userId, orgId, projId, taskId) = await SetupWithTaskAsync("inbox-a@example.com");
        var client = factory.CreateClient();
        async Task Assign() => (await client.SendAsync(Rq(HttpMethod.Put,
            $"/api/v1/organisations/{orgId}/projects/{projId}/tasks/{taskId}/assignee", token, new { assigneeId = userId })))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        await Assign();
        await RunOutboxAsync();

        // List shows one unread notification.
        var page = await (await client.SendAsync(Rq(HttpMethod.Get, "/api/v1/notifications", token))).Content.ReadFromJsonAsync<NotifPage>();
        page!.Items.Should().ContainSingle().Which.IsRead.Should().BeFalse();
        var notifId = page.Items[0].Id;

        (await (await client.SendAsync(Rq(HttpMethod.Get, "/api/v1/notifications/unread-count", token))).Content.ReadFromJsonAsync<UnreadResp>())!.Count.Should().Be(1);

        // Mark the one read -> count drops to zero.
        (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/notifications/{notifId}/read", token))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await (await client.SendAsync(Rq(HttpMethod.Get, "/api/v1/notifications/unread-count", token))).Content.ReadFromJsonAsync<UnreadResp>())!.Count.Should().Be(0);

        // Create another, then mark-all-read clears it.
        await Assign();
        await RunOutboxAsync();
        (await (await client.SendAsync(Rq(HttpMethod.Get, "/api/v1/notifications/unread-count", token))).Content.ReadFromJsonAsync<UnreadResp>())!.Count.Should().Be(1);
        (await client.SendAsync(Rq(HttpMethod.Post, "/api/v1/notifications/read-all", token))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await (await client.SendAsync(Rq(HttpMethod.Get, "/api/v1/notifications/unread-count", token))).Content.ReadFromJsonAsync<UnreadResp>())!.Count.Should().Be(0);
    }

    [Fact]
    public async Task A_user_cannot_see_or_mark_another_users_notifications()
    {
        var (aToken, aUserId, orgId, projId, taskId) = await SetupWithTaskAsync("inbox-owner@example.com");
        var aClient = factory.CreateClient();
        (await aClient.SendAsync(Rq(HttpMethod.Put,
            $"/api/v1/organisations/{orgId}/projects/{projId}/tasks/{taskId}/assignee", aToken, new { assigneeId = aUserId })))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        await RunOutboxAsync();

        var aPage = await (await aClient.SendAsync(Rq(HttpMethod.Get, "/api/v1/notifications", aToken))).Content.ReadFromJsonAsync<NotifPage>();
        var aNotifId = aPage!.Items.Should().ContainSingle().Subject.Id;

        // A second, unrelated user sees nothing and cannot touch A's notification.
        var bClient = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, bClient, "inbox-intruder@example.com");
        var (bToken, _) = await AuthTestHelpers.LoginAsync(bClient, "inbox-intruder@example.com");

        var bPage = await (await bClient.SendAsync(Rq(HttpMethod.Get, "/api/v1/notifications", bToken))).Content.ReadFromJsonAsync<NotifPage>();
        bPage!.Items.Should().BeEmpty();
        (await (await bClient.SendAsync(Rq(HttpMethod.Get, "/api/v1/notifications/unread-count", bToken))).Content.ReadFromJsonAsync<UnreadResp>())!.Count.Should().Be(0);
        (await bClient.SendAsync(Rq(HttpMethod.Post, $"/api/v1/notifications/{aNotifId}/read", bToken))).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
