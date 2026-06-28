using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlowBoard.Infrastructure.Outbox;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration test for activity feeds: domain events dispatched from the outbox are turned into
/// activity-log entries, exposed through the project and user feeds.
/// </summary>
[Collection("Integration")]
public sealed class ActivityFeedTests(FlowBoardApiFactory factory)
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);
    private sealed record ActItem(Guid Id, string EntityType, Guid EntityId, string EventType, Guid? ActorId, DateTime CreatedAt);
    private sealed record ActPage(List<ActItem> Items, string? NextCursor);

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
    public async Task Events_become_activity_entries_in_the_project_and_user_feeds()
    {
        var client = factory.CreateClient();
        var ownerId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "activity-owner@example.com");
        var (token, _) = await AuthTestHelpers.LoginAsync(client, "activity-owner@example.com");

        var orgId = (await (await client.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", token, new { name = "Activity Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Board" }))).Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks", token, new { title = "T", priority = "Medium" }))).Content.ReadFromJsonAsync<TaskBody>())!.Id;
        (await client.SendAsync(Rq(HttpMethod.Put, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks/{taskId}/assignee", token, new { assigneeId = ownerId }))).EnsureSuccessStatusCode();

        await RunOutboxAsync();

        // Project feed: project-level events for this project.
        var projectFeed = await (await client.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}/projects/{projId}/activity", token))).Content.ReadFromJsonAsync<ActPage>();
        projectFeed!.Items.Should().Contain(i => i.EventType == "project.created" && i.EntityType == "Project" && i.EntityId == projId);

        // User feed: the owner's actions, newest first, all attributed to them.
        var userFeed = await (await client.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}/activity", token))).Content.ReadFromJsonAsync<ActPage>();
        userFeed!.Items.Should().OnlyContain(i => i.ActorId == ownerId);
        userFeed.Items.Select(i => i.EventType).Should()
            .Contain(["organisation.created", "project.created", "task.created", "task.assigned"]);
    }
}
