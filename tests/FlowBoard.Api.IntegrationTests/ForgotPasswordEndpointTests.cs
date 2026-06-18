using System.Net;
using System.Net.Http.Json;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Application;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for <c>POST /api/v1/auth/forgot-password</c>, covering the no-enumeration
/// behaviour and reset-token issuance against containerised PostgreSQL and Redis.
/// </summary>
public sealed class ForgotPasswordEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private async Task<string?> PendingResetTokenAsync(Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        return await cache.GetAsync<string>(PasswordResetTokens.PendingKey(UserId.From(userId)));
    }

    [Fact]
    public async Task A_request_for_a_known_verified_account_returns_202_and_stores_a_reset_token()
    {
        var client = factory.CreateClient();
        var userId = await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "forgot-known@example.com");

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new { email = "forgot-known@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await PendingResetTokenAsync(userId)).Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task A_request_for_an_unknown_email_also_returns_202()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new { email = "nobody-here@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task A_malformed_email_is_rejected_with_422()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new { email = "not-an-email" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
