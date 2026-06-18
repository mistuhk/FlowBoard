using System.Net;
using System.Net.Http.Json;
using FlowBoard.Application.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Shared helpers for driving the authentication flow end to end (register, verify, login) and
/// reading the tokens the API returns. The refresh token is delivered as a Secure cookie that the
/// test client will not resend over plain HTTP, so callers attach it to later requests by hand.
/// </summary>
internal static class AuthTestHelpers
{
    public const string Password = "Password1";

    private sealed record RegisterResponse(Guid Id, string Email, string DisplayName);
    private sealed record LoginBody(string AccessToken, DateTime ExpiresAtUtc);

    /// <summary>Registers a user and confirms their email, returning the new user id.</summary>
    public static async Task<Guid> RegisterAndVerifyAsync(
        FlowBoardApiFactory factory, HttpClient client, string email)
    {
        var register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = Password, displayName = "Ada Lovelace" });
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await register.Content.ReadFromJsonAsync<RegisterResponse>())!;

        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var token = await cache.GetAsync<string>($"email_verification:pending:{created.Id}");

        var verify = await client.GetAsync(
            $"/api/v1/auth/verify-email?token={Uri.EscapeDataString(token!)}");
        verify.StatusCode.Should().Be(HttpStatusCode.NoContent);

        return created.Id;
    }

    /// <summary>Logs in and returns the access token and the refresh token from the cookie.</summary>
    public static async Task<(string AccessToken, string RefreshToken)> LoginAsync(
        HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = Password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = (await response.Content.ReadFromJsonAsync<LoginBody>())!;
        var refreshToken = ReadRefreshCookie(response);
        refreshToken.Should().NotBeNullOrEmpty();

        return (body.AccessToken, refreshToken!);
    }

    /// <summary>Reads the <c>refresh_token</c> value from a response's Set-Cookie header.</summary>
    public static string? ReadRefreshCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;

        var cookie = cookies.FirstOrDefault(c => c.StartsWith("refresh_token=", StringComparison.Ordinal));
        if (cookie is null)
            return null;

        var value = cookie.Split(';', 2)[0]["refresh_token=".Length..];
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
