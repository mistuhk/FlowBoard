using System.Net;
using System.Net.Http.Headers;
using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Application;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for <c>POST /api/v1/auth/logout</c>, covering refresh-token revocation and
/// access-token blocklisting against containerised PostgreSQL and Redis.
/// </summary>
public sealed class LogoutEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private static async Task<HttpResponseMessage> LogoutAsync(
        HttpClient client, string accessToken, string? refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (refreshToken is not null)
            request.Headers.Add("Cookie", $"refresh_token={refreshToken}");
        return await client.SendAsync(request);
    }

    private async Task<bool> RefreshTokenExistsAsync(string refreshToken)
    {
        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        return await cache.GetAsync<RefreshTokenEntry>(RefreshTokens.Key(refreshToken)) is not null;
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token_and_blocklists_the_access_token()
    {
        var client = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "logout-ok@example.com");
        var (accessToken, refreshToken) = await AuthTestHelpers.LoginAsync(client, "logout-ok@example.com");

        var logout = await LogoutAsync(client, accessToken, refreshToken);
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The refresh token can no longer be redeemed.
        (await RefreshTokenExistsAsync(refreshToken)).Should().BeFalse();

        // The access token is blocklisted, so a second authenticated call with it is rejected.
        var second = await LogoutAsync(client, accessToken, refreshToken: null);
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_without_a_bearer_token_is_rejected_with_401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/auth/logout", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
