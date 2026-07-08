using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FlowBoard.Application.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for the dashboard endpoint: it aggregates the user's work across the read models,
/// and it is served from the Redis cache (a cache hit does not recompute; assignment invalidates it).
/// </summary>
[Collection("Integration")]
public sealed class DashboardEndpointTests(FlowBoardApiFactory factory)
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);
    private sealed record StatusCountBody(string Status, int Count);
    private sealed record ProjectSummaryBody(Guid Id, string Name, Guid OrganisationId);
    private sealed record DashboardBody(List<StatusCountBody> AssignedByStatus, int OverdueCount, List<ProjectSummaryBody> ActiveProjects, List<object> RecentActivity, double CompletionRate, List<object> Workload);

    private static HttpRequestMessage Rq(HttpMethod m, string uri, string token, object? body = null)
    {
        var r = new HttpRequestMessage(m, uri);
        r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) r.Content = JsonContent.Create(body);
        return r;
    }

    [Fact]
    public async Task Dashboard_aggregates_the_users_work_and_is_served_from_cache()
    {
        var client = factory.CreateClient();
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "dash@example.com");
        var (token, _) = await AuthTestHelpers.LoginAsync(client, "dash@example.com");

        var orgId = (await (await client.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", token, new { name = "Dash Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Board" }))).Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        var taskId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks", token, new { title = "T", priority = "Medium" }))).Content.ReadFromJsonAsync<TaskBody>())!.Id;
        await client.SendAsync(Rq(HttpMethod.Put, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks/{taskId}/assignee", token, new { assigneeId = userId }));

        // First read: computes and caches. The assigned task shows under its status.
        var first = await client.SendAsync(Rq(HttpMethod.Get, "/api/v1/dashboard", token));
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var dash = (await first.Content.ReadFromJsonAsync<DashboardBody>())!;
        dash.AssignedByStatus.Should().Contain(s => s.Status == "todo" && s.Count == 1);

        // The cache now holds this user's dashboard.
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var cached = await cache.GetAsync<object>($"{userId}:dashboard");
            cached.Should().NotBeNull();
        }

        // Second read is served from cache and matches.
        var second = await (await client.SendAsync(Rq(HttpMethod.Get, "/api/v1/dashboard", token))).Content.ReadFromJsonAsync<DashboardBody>();
        second!.AssignedByStatus.Should().Contain(s => s.Status == "todo" && s.Count == 1);
        second.ActiveProjects.Should().ContainSingle(p => p.Id == projId);
    }

    [Fact]
    public async Task Dashboard_requires_authentication()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
