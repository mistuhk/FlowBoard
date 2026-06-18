using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Identity.Application;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for <c>POST /api/v1/auth/login</c>, exercising the full pipeline against
/// containerised PostgreSQL and Redis.
/// </summary>
public sealed class LoginEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private const string Password = "Password1";

    private sealed record RegisterResponse(Guid Id, string Email, string DisplayName);
    private sealed record LoginBody(string AccessToken, DateTime ExpiresAtUtc);

    private async Task<Guid> RegisterAsync(HttpClient client, string email, bool verify)
    {
        var register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = Password, displayName = "Ada Lovelace" });
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await register.Content.ReadFromJsonAsync<RegisterResponse>())!;

        if (verify)
        {
            using var scope = factory.Services.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var token = await cache.GetAsync<string>($"email_verification:pending:{created.Id}");
            var verifyResponse = await client.GetAsync(
                $"/api/v1/auth/verify-email?token={Uri.EscapeDataString(token!)}");
            verifyResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        return created.Id;
    }

    [Fact]
    public async Task A_verified_user_with_correct_credentials_receives_an_access_token_and_a_refresh_cookie()
    {
        var client = factory.CreateClient();
        var email = "login-ok@example.com";
        var userId = await RegisterAsync(client, email, verify: true);

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = (await response.Content.ReadFromJsonAsync<LoginBody>())!;
        body.AccessToken.Should().NotBeNullOrEmpty();
        body.ExpiresAtUtc.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromMinutes(1));

        // The access token carries the user's id (sub), email, and a unique token id (jti).
        var claims = DecodeJwtPayload(body.AccessToken);
        claims.GetProperty("sub").GetString().Should().Be(userId.ToString());
        claims.GetProperty("email").GetString().Should().Be(email);
        claims.GetProperty("jti").GetString().Should().NotBeNullOrEmpty();

        // The refresh token is set as an httpOnly cookie and stored server-side against the user.
        var refreshToken = ReadRefreshCookie(response);
        refreshToken.Should().NotBeNullOrEmpty();

        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var storedEntry = await cache.GetAsync<RefreshTokenEntry>(RefreshTokens.Key(refreshToken!));
        storedEntry!.UserId.Should().Be(userId.ToString());
    }

    [Fact]
    public async Task A_wrong_password_is_rejected_with_401()
    {
        var client = factory.CreateClient();
        var email = "login-wrong-pw@example.com";
        await RegisterAsync(client, email, verify: true);

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = "WrongPassword1" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task An_unknown_email_is_rejected_with_401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "nobody@example.com", password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unverified_account_is_rejected_with_403()
    {
        var client = factory.CreateClient();
        var email = "login-unverified@example.com";
        await RegisterAsync(client, email, verify: false);

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_malformed_email_is_rejected_with_422()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "not-an-email", password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private static string? ReadRefreshCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;

        var cookie = cookies.FirstOrDefault(c => c.StartsWith("refresh_token=", StringComparison.Ordinal));
        if (cookie is null)
            return null;

        // "refresh_token=<value>; Path=...; HttpOnly; ..." -> take the value before the first ';'.
        var firstPair = cookie.Split(';', 2)[0];
        return firstPair["refresh_token=".Length..];
    }

    private static JsonElement DecodeJwtPayload(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var padded = payload.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        return JsonDocument.Parse(json).RootElement.Clone();
    }
}
