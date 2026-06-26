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
/// Integration test for the cross-module assignment-notification flow: assigning a task raises a
/// domain event that, once dispatched from the outbox, the Notifications module turns into a
/// notification. Also covers rejecting a non-member assignee.
/// </summary>
public sealed class TaskAssignmentNotificationTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);

    private static HttpRequestMessage Request(HttpMethod method, string uri, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return request;
    }

    private async Task RunOutboxAsync()
    {
        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<OutboxProcessor>().RunAsync();
    }

    [Fact]
    public async Task Assigning_a_task_creates_a_notification_for_the_assignee()
    {
        var client = factory.CreateClient();
        var ownerId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "assign-notify@example.com");
        var (token, _) = await AuthTestHelpers.LoginAsync(client, "assign-notify@example.com");

        var orgId = (await (await client.SendAsync(
            Request(HttpMethod.Post, "/api/v1/organisations", token, new { name = "Notify Co" })))
            .Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projectId = (await (await client.SendAsync(
            Request(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Board" })))
            .Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await client.SendAsync(
            Request(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projectId}/tasks", token,
                new { title = "Ship it", priority = "High" })))
            .Content.ReadFromJsonAsync<TaskBody>())!.Id;

        // Assign the task to the owner (a member of the organisation).
        (await client.SendAsync(Request(
            HttpMethod.Put, $"/api/v1/organisations/{orgId}/projects/{projectId}/tasks/{taskId}/assignee",
            token, new { assigneeId = ownerId })))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Dispatch the outbox; the Notifications module reacts to TaskAssignedEvent.
        await RunOutboxAsync();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notification = await db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.UserId == UserId.From(ownerId) && n.EntityId == taskId);

        notification.Should().NotBeNull();
        notification!.Type.Name.Should().Be("TaskAssigned");
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task Assigning_a_task_to_a_non_member_is_rejected()
    {
        var client = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "assign-nonmember@example.com");
        var (token, _) = await AuthTestHelpers.LoginAsync(client, "assign-nonmember@example.com");

        var orgId = (await (await client.SendAsync(
            Request(HttpMethod.Post, "/api/v1/organisations", token, new { name = "NonMember Co" })))
            .Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projectId = (await (await client.SendAsync(
            Request(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Board" })))
            .Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await client.SendAsync(
            Request(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projectId}/tasks", token,
                new { title = "Task", priority = "Low" })))
            .Content.ReadFromJsonAsync<TaskBody>())!.Id;

        var response = await client.SendAsync(Request(
            HttpMethod.Put, $"/api/v1/organisations/{orgId}/projects/{projectId}/tasks/{taskId}/assignee",
            token, new { assigneeId = Guid.NewGuid() }));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
