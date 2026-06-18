using System.Net;
using System.Net.Http.Json;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for <c>GET /api/v1/auth/verify-email</c>, exercising the full pipeline
/// against containerised PostgreSQL and Redis.
/// </summary>
public sealed class VerifyEmailEndpointTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private sealed record RegisterResponse(Guid Id, string Email, string DisplayName);

    private async Task<(Guid Id, string Token)> RegisterAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Password1", displayName = "Ada Lovelace" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<RegisterResponse>())!;

        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var token = await cache.GetAsync<string>($"email_verification:pending:{created.Id}");
        token.Should().NotBeNullOrEmpty();

        return (created.Id, token!);
    }

    private async Task<bool> IsEmailVerifiedAsync(Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await context.Set<User>().FirstOrDefaultAsync(u => u.Id == UserId.From(userId));
        return user!.IsEmailVerified;
    }

    [Fact]
    public async Task A_valid_token_verifies_the_account_and_is_single_use()
    {
        var client = factory.CreateClient();
        var (id, token) = await RegisterAsync(client, "verify-me@example.com");

        var first = await client.GetAsync($"/api/v1/auth/verify-email?token={Uri.EscapeDataString(token)}");
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await IsEmailVerifiedAsync(id)).Should().BeTrue();

        // The token is consumed, so a second attempt is rejected.
        var second = await client.GetAsync($"/api/v1/auth/verify-email?token={Uri.EscapeDataString(token)}");
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        second.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task An_unknown_token_returns_400_bad_request()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/verify-email?token=does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_empty_token_is_rejected_as_a_bad_request()
    {
        var client = factory.CreateClient();

        // The token query parameter is implicitly required, so an empty value is rejected
        // at model binding with a 400 before the handler runs.
        var response = await client.GetAsync("/api/v1/auth/verify-email?token=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
