using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for the project endpoints under
/// <c>/api/v1/organisations/{orgId}/projects</c>, exercising the full lifecycle and tenant
/// isolation against containerised PostgreSQL and Redis.
/// </summary>
public sealed class ProjectsEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record ProjectBody(
        Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);

    private async Task<(HttpClient Client, string Token)> AuthenticatedAsync(string email)
    {
        var client = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, email);
        var (token, _) = await AuthTestHelpers.LoginAsync(client, email);
        return (client, token);
    }

    private static HttpRequestMessage Request(HttpMethod method, string uri, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return request;
    }

    private static async Task<Guid> CreateOrganisationAsync(HttpClient client, string token, string name)
    {
        var response = await client.SendAsync(Request(HttpMethod.Post, "/api/v1/organisations", token, new { name }));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<OrgBody>())!.Id;
    }

    [Fact]
    public async Task Full_project_lifecycle_succeeds_for_the_organisation_owner()
    {
        var (client, token) = await AuthenticatedAsync("projects-owner@example.com");
        var orgId = await CreateOrganisationAsync(client, token, "Acme Projects");
        var basePath = $"/api/v1/organisations/{orgId}/projects";

        // Create
        var create = await client.SendAsync(
            Request(HttpMethod.Post, basePath, token, new { name = "Website", description = "Public site" }));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = (await create.Content.ReadFromJsonAsync<ProjectBody>())!;
        project.Status.Should().Be("Active");
        project.OrganisationId.Should().Be(orgId);
        var projectPath = $"{basePath}/{project.Id}";

        // Read (single + list)
        (await client.SendAsync(Request(HttpMethod.Get, projectPath, token))).StatusCode
            .Should().Be(HttpStatusCode.OK);
        var list = await (await client.SendAsync(Request(HttpMethod.Get, basePath, token)))
            .Content.ReadFromJsonAsync<List<ProjectBody>>();
        list!.Should().ContainSingle(p => p.Id == project.Id);

        // Update
        var update = await client.SendAsync(
            Request(HttpMethod.Put, projectPath, token, new { name = "Website Revamp", description = "v2" }));
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<ProjectBody>())!.Name.Should().Be("Website Revamp");

        // Archive, then edits are rejected with 422
        (await client.SendAsync(Request(HttpMethod.Put, $"{projectPath}/archive", token))).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
        (await client.SendAsync(Request(HttpMethod.Put, projectPath, token, new { name = "Nope", description = (string?)null })))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Restore, then delete
        (await client.SendAsync(Request(HttpMethod.Put, $"{projectPath}/restore", token))).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
        (await client.SendAsync(Request(HttpMethod.Delete, projectPath, token))).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        // A deleted project is gone
        (await client.SendAsync(Request(HttpMethod.Get, projectPath, token))).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_user_who_is_not_a_member_cannot_access_another_organisations_projects()
    {
        var (ownerClient, ownerToken) = await AuthenticatedAsync("projects-org-a@example.com");
        var orgAId = await CreateOrganisationAsync(ownerClient, ownerToken, "Org A");

        // Outsider belongs only to their own organisation, not Org A.
        var (outsiderClient, outsiderToken) = await AuthenticatedAsync("projects-org-b@example.com");
        await CreateOrganisationAsync(outsiderClient, outsiderToken, "Org B");

        var listForbidden = await outsiderClient.SendAsync(
            Request(HttpMethod.Get, $"/api/v1/organisations/{orgAId}/projects", outsiderToken));

        listForbidden.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Creating_a_project_requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/organisations/{Guid.NewGuid()}/projects", new { name = "X" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
