using System.Net;
using System.Net.Http.Json;
using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Application;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for <c>POST /api/v1/auth/refresh</c>, exercising refresh-token rotation
/// against containerised PostgreSQL and Redis.
/// </summary>
public sealed class RefreshEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record LoginBody(string AccessToken, DateTime ExpiresAtUtc);

    private static async Task<HttpResponseMessage> RefreshWithCookieAsync(HttpClient client, string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        request.Headers.Add("Cookie", $"refresh_token={refreshToken}");
        return await client.SendAsync(request);
    }

    private async Task<string?> StoredUserIdForAsync(string refreshToken)
    {
        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        return await cache.GetAsync<string>(RefreshTokens.Key(refreshToken));
    }

    [Fact]
    public async Task A_valid_refresh_cookie_returns_a_new_access_token_and_rotates_the_refresh_token()
    {
        var client = factory.CreateClient();
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "refresh-ok@example.com");
        var (_, refreshToken) = await AuthTestHelpers.LoginAsync(client, "refresh-ok@example.com");

        var response = await RefreshWithCookieAsync(client, refreshToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<LoginBody>())!;
        body.AccessToken.Should().NotBeNullOrEmpty();

        var newRefreshToken = AuthTestHelpers.ReadRefreshCookie(response);
        newRefreshToken.Should().NotBeNullOrEmpty().And.NotBe(refreshToken);

        // The old token is consumed and the new one maps to the same user.
        (await StoredUserIdForAsync(refreshToken)).Should().BeNull();
        (await StoredUserIdForAsync(newRefreshToken!)).Should().Be(userId.ToString());
    }

    [Fact]
    public async Task A_consumed_refresh_token_cannot_be_used_again()
    {
        var client = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "refresh-replay@example.com");
        var (_, refreshToken) = await AuthTestHelpers.LoginAsync(client, "refresh-replay@example.com");

        var first = await RefreshWithCookieAsync(client, refreshToken);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await RefreshWithCookieAsync(client, refreshToken);
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        second.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task A_request_without_a_refresh_cookie_is_rejected_with_401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/auth/refresh", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unknown_refresh_token_is_rejected_with_401()
    {
        var client = factory.CreateClient();

        var response = await RefreshWithCookieAsync(client, "does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
