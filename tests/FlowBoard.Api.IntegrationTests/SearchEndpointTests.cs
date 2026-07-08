using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for full-text search over real PostgreSQL: text queries rank and match tasks and
/// projects, and a filter-only query (no text) returns filtered results without FTS scoring.
/// </summary>
[Collection("Integration")]
public sealed class SearchEndpointTests(FlowBoardApiFactory factory)
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);
    private sealed record TaskBody(Guid Id, Guid ProjectId, Guid OrganisationId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, Guid CreatedById, DateTime? DueDate, DateTime CreatedAt);
    private sealed record TaskHit(Guid Id, Guid ProjectId, string Title, string Status, string Priority, double Rank);
    private sealed record ProjHit(Guid Id, string Name, string Status, double Rank);
    private sealed record SearchBody(List<TaskHit> Tasks, List<ProjHit> Projects);

    private static HttpRequestMessage Rq(HttpMethod m, string uri, string token, object? body = null)
    {
        var r = new HttpRequestMessage(m, uri);
        r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) r.Content = JsonContent.Create(body);
        return r;
    }

    [Fact]
    public async Task Full_text_search_ranks_matches_and_filter_only_skips_scoring()
    {
        var client = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "search-owner@example.com");
        var (token, _) = await AuthTestHelpers.LoginAsync(client, "search-owner@example.com");

        var orgId = (await (await client.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", token, new { name = "Search Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projId = (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Marketing Website" }))).Content.ReadFromJsonAsync<ProjectBody>())!.Id;
        await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", token, new { name = "Internal Tooling" }));

        async Task<Guid> CreateTask(string title, string description) =>
            (await (await client.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projId}/tasks", token,
                new { title, description, priority = "Medium" }))).Content.ReadFromJsonAsync<TaskBody>())!.Id;

        var loginTaskId = await CreateTask("Fix the login bug", "Users cannot authenticate");
        await CreateTask("Redesign the landing page", "Improve the marketing hero section");

        // Text query: "login" matches only the login task.
        var byLogin = (await (await client.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}/search?q=login", token))).Content.ReadFromJsonAsync<SearchBody>())!;
        byLogin.Tasks.Should().ContainSingle(t => t.Id == loginTaskId);
        byLogin.Tasks[0].Rank.Should().BeGreaterThan(0);

        // Text query: "marketing" matches the project by name and the redesign task by description.
        var byMarketing = (await (await client.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}/search?q=marketing", token))).Content.ReadFromJsonAsync<SearchBody>())!;
        byMarketing.Projects.Should().Contain(p => p.Name == "Marketing Website");
        byMarketing.Tasks.Should().Contain(t => t.Title == "Redesign the landing page");

        // Filter-only (no text): all Todo tasks, unranked.
        var todo = (await (await client.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}/search?type=tasks&status=Todo", token))).Content.ReadFromJsonAsync<SearchBody>())!;
        todo.Tasks.Should().HaveCount(2);
        todo.Tasks.Should().OnlyContain(t => t.Rank == 0);
    }
}
