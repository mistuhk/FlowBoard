using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for project membership and Guest access enforcement: a Guest sees a project
/// only once they have been explicitly added to it, and an unseen project reports 404 (not 403).
/// </summary>
public sealed class ProjectMembersEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record InvitationBody(
        Guid InvitationId, Guid OrganisationId, string InvitedEmail, string Role, DateTime ExpiresAt, string Token);
    private sealed record ProjectBody(
        Guid Id, Guid OrganisationId, string Name, string? Description, string Status, Guid CreatedById);

    private static HttpRequestMessage Request(HttpMethod method, string uri, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return request;
    }

    [Fact]
    public async Task A_guest_sees_a_project_only_after_being_added_to_it()
    {
        // Owner sets up an organisation and a project.
        var ownerClient = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, ownerClient, "guest-test-owner@example.com");
        var (ownerToken, _) = await AuthTestHelpers.LoginAsync(ownerClient, "guest-test-owner@example.com");

        var orgId = (await (await ownerClient.SendAsync(
            Request(HttpMethod.Post, "/api/v1/organisations", ownerToken, new { name = "Guest Co" })))
            .Content.ReadFromJsonAsync<OrgBody>())!.Id;

        var projectId = (await (await ownerClient.SendAsync(
            Request(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", ownerToken, new { name = "Secret" })))
            .Content.ReadFromJsonAsync<ProjectBody>())!.Id;

        // Owner invites a Guest.
        var invite = await ownerClient.SendAsync(Request(
            HttpMethod.Post, $"/api/v1/organisations/{orgId}/invitations", ownerToken,
            new { email = "guest-test-guest@example.com", role = "Guest" }));
        invite.StatusCode.Should().Be(HttpStatusCode.Created);
        var token = (await invite.Content.ReadFromJsonAsync<InvitationBody>())!.Token;

        // Guest registers, logs in, and accepts the invitation.
        var guestClient = factory.CreateClient();
        var guestUserId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, guestClient, "guest-test-guest@example.com");
        var (guestToken, _) = await AuthTestHelpers.LoginAsync(guestClient, "guest-test-guest@example.com");
        (await guestClient.SendAsync(Request(
            HttpMethod.Post, "/api/v1/invitations/accept", guestToken, new { token })))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var projectPath = $"/api/v1/organisations/{orgId}/projects/{projectId}";

        // Before being added, the Guest cannot see the project (404, not 403) and lists nothing.
        (await guestClient.SendAsync(Request(HttpMethod.Get, projectPath, guestToken)))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        var emptyList = await (await guestClient.SendAsync(Request(
            HttpMethod.Get, $"/api/v1/organisations/{orgId}/projects", guestToken)))
            .Content.ReadFromJsonAsync<List<ProjectBody>>();
        emptyList!.Should().BeEmpty();

        // Owner adds the Guest to the project.
        (await ownerClient.SendAsync(Request(
            HttpMethod.Post, $"{projectPath}/members", ownerToken, new { userId = guestUserId })))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Now the Guest can see it.
        (await guestClient.SendAsync(Request(HttpMethod.Get, projectPath, guestToken)))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var visibleList = await (await guestClient.SendAsync(Request(
            HttpMethod.Get, $"/api/v1/organisations/{orgId}/projects", guestToken)))
            .Content.ReadFromJsonAsync<List<ProjectBody>>();
        visibleList!.Should().ContainSingle(p => p.Id == projectId);
    }

    [Fact]
    public async Task A_non_member_cannot_be_added_to_a_project()
    {
        var ownerClient = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, ownerClient, "pm-owner@example.com");
        var (ownerToken, _) = await AuthTestHelpers.LoginAsync(ownerClient, "pm-owner@example.com");

        var orgId = (await (await ownerClient.SendAsync(
            Request(HttpMethod.Post, "/api/v1/organisations", ownerToken, new { name = "PM Co" })))
            .Content.ReadFromJsonAsync<OrgBody>())!.Id;
        var projectId = (await (await ownerClient.SendAsync(
            Request(HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects", ownerToken, new { name = "Proj" })))
            .Content.ReadFromJsonAsync<ProjectBody>())!.Id;

        // A random user who is not a member of the organisation cannot be added.
        var response = await ownerClient.SendAsync(Request(
            HttpMethod.Post, $"/api/v1/organisations/{orgId}/projects/{projectId}/members", ownerToken,
            new { userId = Guid.NewGuid() }));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
