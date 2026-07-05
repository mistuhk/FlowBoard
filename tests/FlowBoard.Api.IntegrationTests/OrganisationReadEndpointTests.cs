using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for the organisation read endpoints added for the frontend: list my
/// organisations, get one (with the caller's role), and list members (member-only).
/// </summary>
[Collection("Integration")]
public sealed class OrganisationReadEndpointTests(FlowBoardApiFactory factory)
{
    private sealed record OrgBody(Guid Id, string Name, string Slug, Guid OwnerId);
    private sealed record OrgSummary(Guid Id, string Name, string Slug, Guid OwnerId, string Role);
    private sealed record MemberBody(Guid UserId, string Email, string DisplayName, string Role, DateTime JoinedAt);
    private sealed record InvitationBody(Guid InvitationId, Guid OrganisationId, string InvitedEmail, string Role, DateTime ExpiresAt, string Token);

    private static HttpRequestMessage Rq(HttpMethod m, string uri, string token, object? body = null)
    {
        var r = new HttpRequestMessage(m, uri);
        r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) r.Content = JsonContent.Create(body);
        return r;
    }

    [Fact]
    public async Task Lists_my_organisations_and_a_single_one_with_my_role()
    {
        var client = factory.CreateClient();
        var ownerId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "org-read-owner@example.com");
        var (token, _) = await AuthTestHelpers.LoginAsync(client, "org-read-owner@example.com");

        var orgId = (await (await client.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", token, new { name = "Read Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;

        var list = await (await client.SendAsync(Rq(HttpMethod.Get, "/api/v1/organisations", token))).Content.ReadFromJsonAsync<List<OrgSummary>>();
        list!.Should().ContainSingle(o => o.Id == orgId).Which.Role.Should().Be("owner");

        var one = await client.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}", token));
        one.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = (await one.Content.ReadFromJsonAsync<OrgSummary>())!;
        summary.OwnerId.Should().Be(ownerId);
        summary.Role.Should().Be("owner");
    }

    [Fact]
    public async Task A_non_member_cannot_read_the_organisation_or_its_members()
    {
        var owner = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, owner, "org-read-a@example.com");
        var (ownerTok, _) = await AuthTestHelpers.LoginAsync(owner, "org-read-a@example.com");
        var orgId = (await (await owner.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", ownerTok, new { name = "Private Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;

        var outsider = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, outsider, "org-read-outsider@example.com");
        var (outsiderTok, _) = await AuthTestHelpers.LoginAsync(outsider, "org-read-outsider@example.com");

        (await outsider.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}", outsiderTok))).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await outsider.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}/members", outsiderTok))).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Lists_members_with_their_roles()
    {
        var owner = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, owner, "org-members-owner@example.com");
        var (ownerTok, _) = await AuthTestHelpers.LoginAsync(owner, "org-members-owner@example.com");
        var orgId = (await (await owner.SendAsync(Rq(HttpMethod.Post, "/api/v1/organisations", ownerTok, new { name = "Members Co" }))).Content.ReadFromJsonAsync<OrgBody>())!.Id;

        // Invite and accept a second member.
        var inv = await owner.SendAsync(Rq(HttpMethod.Post, $"/api/v1/organisations/{orgId}/invitations", ownerTok, new { email = "org-members-bob@example.com", role = "Member" }));
        var invToken = (await inv.Content.ReadFromJsonAsync<InvitationBody>())!.Token;
        var bob = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, bob, "org-members-bob@example.com");
        var (bobTok, _) = await AuthTestHelpers.LoginAsync(bob, "org-members-bob@example.com");
        (await bob.SendAsync(Rq(HttpMethod.Post, "/api/v1/invitations/accept", bobTok, new { token = invToken }))).StatusCode.Should().Be(HttpStatusCode.OK);

        var members = await (await owner.SendAsync(Rq(HttpMethod.Get, $"/api/v1/organisations/{orgId}/members", ownerTok))).Content.ReadFromJsonAsync<List<MemberBody>>();
        members!.Should().HaveCount(2);
        members.Should().Contain(m => m.Role == "owner" && m.Email == "org-members-owner@example.com");
        members.Should().Contain(m => m.Role == "member" && m.Email == "org-members-bob@example.com");
    }
}
