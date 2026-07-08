using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for the task endpoints under
/// <c>/api/v1/organisations/{orgId}/projects/{projectId}/tasks</c>: lifecycle, the status state
/// machine, archived-project rejection, and cursor pagination.
/// </summary>
[Collection("Integration")]
public sealed class TasksEndpointTests(FlowBoardApiFactory factory)
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(
        Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description,
        string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);
    private sealed record TaskPage(List<TaskBody> Items, string? NextCursor);

    private static HttpRequestMessage Request(HttpMethod method, string uri, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return request;
    }

    private async Task<(HttpClient Client, string Token, Guid UserId, Guid OrgId, Guid ProjectId)> SetUpAsync(string email)
    {
        var client = factory.CreateClient();
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, email);
        var (token, _) = await AuthTestHelpers.LoginAsync(client, email);

        // A distinct organisation name per test keeps the unique slug from colliding across the
        // shared database.
        var orgName = email.Split('@')[0];
        var orgId = (await (await client.SendAsync(
            Request(HttpMethod.Post, "/api/v1/organisations", token, new { name = orgName })))
            .Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projectId = (await (await client.SendAsync(
            Request(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Board" })))
            .Content.ReadFromJsonAsync<ProjectBody>())!.Id;

        return (client, token, userId, orgId, projectId);
    }

    [Fact]
    public async Task Full_task_lifecycle_and_status_machine()
    {
        var (client, token, userId, orgId, projectId) = await SetUpAsync("tasks-lifecycle@example.com");
        var basePath = $"/api/v1/organisations/{orgId}/projects/{projectId}/tasks";

        var create = await client.SendAsync(Request(
            HttpMethod.Post, basePath, token, new { title = "Design API", priority = "High" }));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = (await create.Content.ReadFromJsonAsync<TaskBody>())!;
        task.Status.Should().Be("Todo");
        task.Priority.Should().Be("High");
        var taskPath = $"{basePath}/{task.Id}";

        // Valid transition Todo -> InProgress.
        var inProgress = await client.SendAsync(Request(
            HttpMethod.Put, $"{taskPath}/status", token, new { status = "InProgress" }));
        inProgress.StatusCode.Should().Be(HttpStatusCode.OK);
        (await inProgress.Content.ReadFromJsonAsync<TaskBody>())!.Status.Should().Be("InProgress");

        // Invalid transition InProgress -> Todo is allowed, but Done -> Blocked is not; verify an
        // invalid one returns 422. From InProgress, going to Todo then Done-from-Todo is invalid.
        (await client.SendAsync(Request(HttpMethod.Put, $"{taskPath}/status", token, new { status = "Todo" })))
            .StatusCode.Should().Be(HttpStatusCode.OK);  // InProgress -> Todo (valid)
        (await client.SendAsync(Request(HttpMethod.Put, $"{taskPath}/status", token, new { status = "Done" })))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);  // Todo -> Done (invalid)

        // Assign, change priority, then unassign and delete.
        (await client.SendAsync(Request(HttpMethod.Put, $"{taskPath}/assignee", token, new { assigneeId = userId })))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.SendAsync(Request(HttpMethod.Put, $"{taskPath}/priority", token, new { priority = "Low" })))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.SendAsync(Request(HttpMethod.Delete, $"{taskPath}/assignee", token)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.SendAsync(Request(HttpMethod.Delete, taskPath, token)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The deleted task no longer appears in the list.
        var page = await (await client.SendAsync(Request(HttpMethod.Get, basePath, token)))
            .Content.ReadFromJsonAsync<TaskPage>();
        page!.Items.Should().NotContain(t => t.Id == task.Id);
    }

    [Fact]
    public async Task Creating_a_task_in_an_archived_project_is_rejected()
    {
        var (client, token, _, orgId, projectId) = await SetUpAsync("tasks-archived@example.com");

        (await client.SendAsync(Request(
            HttpMethod.Put, $"/api/v1/organisations/{orgId}/projects/{projectId}/archive", token)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var create = await client.SendAsync(Request(
            HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projectId}/tasks", token,
            new { title = "Too late", priority = "Medium" }));

        create.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Listing_tasks_pages_with_an_opaque_cursor()
    {
        var (client, token, _, orgId, projectId) = await SetUpAsync("tasks-paging@example.com");
        var basePath = $"/api/v1/organisations/{orgId}/projects/{projectId}/tasks";

        for (var i = 0; i < 3; i++)
        {
            (await client.SendAsync(Request(HttpMethod.Post, basePath, token, new { title = $"Task {i}", priority = "Medium" })))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var first = await (await client.SendAsync(Request(HttpMethod.Get, $"{basePath}?limit=2", token)))
            .Content.ReadFromJsonAsync<TaskPage>();
        first!.Items.Should().HaveCount(2);
        first.NextCursor.Should().NotBeNull();

        var second = await (await client.SendAsync(Request(
            HttpMethod.Get, $"{basePath}?limit=2&cursor={Uri.EscapeDataString(first.NextCursor!)}", token)))
            .Content.ReadFromJsonAsync<TaskPage>();
        second!.Items.Should().ContainSingle();
        second.NextCursor.Should().BeNull();

        // No task appears on both pages.
        first.Items.Select(t => t.Id).Should().NotIntersectWith(second.Items.Select(t => t.Id));
    }
}
