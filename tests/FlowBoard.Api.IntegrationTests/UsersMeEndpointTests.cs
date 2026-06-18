using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for the profile endpoints <c>GET/PUT /api/v1/users/me</c>, exercising the JWT
/// bearer identity and profile update against containerised PostgreSQL and Redis.
/// </summary>
public sealed class UsersMeEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record ProfileBody(
        Guid Id,
        string Email,
        string DisplayName,
        string? AvatarUrl,
        bool IsEmailVerified,
        DateTime CreatedAt,
        DateTime? LastLoginAt);

    private async Task<(HttpClient Client, string AccessToken, Guid UserId)> AuthenticatedAsync(string email)
    {
        var client = factory.CreateClient();
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, email);
        var (accessToken, _) = await AuthTestHelpers.LoginAsync(client, email);
        return (client, accessToken, userId);
    }

    private static HttpRequestMessage Authorised(HttpMethod method, string uri, string accessToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return request;
    }

    [Fact]
    public async Task Get_me_returns_the_authenticated_users_profile()
    {
        var (client, accessToken, userId) = await AuthenticatedAsync("me-get@example.com");

        var response = await client.SendAsync(Authorised(HttpMethod.Get, "/api/v1/users/me", accessToken));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = (await response.Content.ReadFromJsonAsync<ProfileBody>())!;
        profile.Id.Should().Be(userId);
        profile.Email.Should().Be("me-get@example.com");
        profile.DisplayName.Should().Be("Ada Lovelace");
        profile.IsEmailVerified.Should().BeTrue();
        profile.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_me_without_a_token_is_rejected_with_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_me_updates_the_display_name_and_the_change_is_persisted()
    {
        var (client, accessToken, _) = await AuthenticatedAsync("me-put@example.com");

        var update = await client.SendAsync(
            Authorised(HttpMethod.Put, "/api/v1/users/me", accessToken, new { displayName = "Augusta King" }));

        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await update.Content.ReadFromJsonAsync<ProfileBody>())!;
        updated.DisplayName.Should().Be("Augusta King");

        // A fresh read reflects the change.
        var get = await client.SendAsync(Authorised(HttpMethod.Get, "/api/v1/users/me", accessToken));
        var profile = (await get.Content.ReadFromJsonAsync<ProfileBody>())!;
        profile.DisplayName.Should().Be("Augusta King");
    }

    [Fact]
    public async Task Put_me_with_an_empty_display_name_is_rejected_with_422()
    {
        var (client, accessToken, _) = await AuthenticatedAsync("me-put-invalid@example.com");

        var response = await client.SendAsync(
            Authorised(HttpMethod.Put, "/api/v1/users/me", accessToken, new { displayName = "" }));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
