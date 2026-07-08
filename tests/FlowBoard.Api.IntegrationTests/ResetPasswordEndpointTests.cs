using System.Net;
using System.Net.Http.Json;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for <c>POST /api/v1/auth/reset-password</c>, covering a full reset, token
/// single-use, and revocation of pre-reset refresh tokens, against containerised PostgreSQL and Redis.
/// </summary>
[Collection("Integration")]
public sealed class ResetPasswordEndpointTests(FlowBoardApiFactory factory)
{
    private const string NewPassword = "NewPassword1";

    private async Task<string> RequestResetTokenAsync(HttpClient client, Guid userId, string email)
    {
        var forgot = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });
        forgot.StatusCode.Should().Be(HttpStatusCode.Accepted);

        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var token = await cache.GetAsync<string>(PasswordResetTokens.PendingKey(UserId.From(userId)));
        token.Should().NotBeNullOrEmpty();
        return token!;
    }

    private static Task<HttpResponseMessage> ResetAsync(HttpClient client, string token, string newPassword) =>
        client.PostAsJsonAsync("/api/v1/auth/reset-password", new { token, newPassword });

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });

    private static Task<HttpResponseMessage> RefreshAsync(HttpClient client, string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        request.Headers.Add("Cookie", $"refresh_token={refreshToken}");
        return client.SendAsync(request);
    }

    [Fact]
    public async Task A_valid_reset_changes_the_password_and_revokes_existing_sessions()
    {
        var client = factory.CreateClient();
        const string email = "reset-ok@example.com";
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, email);

        // A session that exists before the reset, to prove it is revoked afterwards.
        var (_, preResetRefreshToken) = await AuthTestHelpers.LoginAsync(client, email);

        var token = await RequestResetTokenAsync(client, userId, email);

        var reset = await ResetAsync(client, token, NewPassword);
        reset.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The old password no longer works; the new one does.
        (await LoginAsync(client, email, AuthTestHelpers.Password)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await LoginAsync(client, email, NewPassword)).StatusCode.Should().Be(HttpStatusCode.OK);

        // The refresh token issued before the reset can no longer be redeemed.
        (await RefreshAsync(client, preResetRefreshToken)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_reset_token_is_single_use()
    {
        var client = factory.CreateClient();
        const string email = "reset-single-use@example.com";
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, email);
        var token = await RequestResetTokenAsync(client, userId, email);

        (await ResetAsync(client, token, NewPassword)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var second = await ResetAsync(client, token, "AnotherPass1");
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        second.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task An_unknown_reset_token_is_rejected_with_400()
    {
        var client = factory.CreateClient();

        var response = await ResetAsync(client, "does-not-exist", NewPassword);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_weak_new_password_is_rejected_with_422()
    {
        var client = factory.CreateClient();
        const string email = "reset-weak@example.com";
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, email);
        var token = await RequestResetTokenAsync(client, userId, email);

        var response = await ResetAsync(client, token, "weak");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
