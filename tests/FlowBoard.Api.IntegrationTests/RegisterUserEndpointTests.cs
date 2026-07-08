using System.Net;
using System.Net.Http.Json;
using FlowBoard.Application.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for <c>POST /api/v1/auth/register</c>, exercising the full pipeline
/// against containerised PostgreSQL and Redis.
/// </summary>
[Collection("Integration")]
public sealed class RegisterUserEndpointTests(FlowBoardApiFactory factory)
{
    private sealed record RegisterResponse(Guid Id, string Email, string DisplayName);

    private static object Body(string email, string password = "Password1", string displayName = "Ada Lovelace") =>
        new { email, password, displayName };

    [Fact]
    public async Task Registering_with_a_valid_body_returns_201_and_stores_a_24h_verification_token()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register", Body("valid@example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        created.Should().NotBeNull();
        created!.Email.Should().Be("valid@example.com");
        created.DisplayName.Should().Be("Ada Lovelace");
        created.Id.Should().NotBe(Guid.Empty);

        var pendingKey = $"email_verification:pending:{created.Id}";

        // Assert through the same cache contract the app (and email verification) uses,
        // so JSON encoding is handled consistently.
        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var token = await cache.GetAsync<string>(pendingKey);
        token.Should().NotBeNullOrEmpty();

        var mappedUserId = await cache.GetAsync<string>($"email_verification:token:{token}");
        mappedUserId.Should().Be(created.Id.ToString());

        // TTL is not exposed by the cache abstraction, so check it directly.
        await using var redis = await ConnectionMultiplexer.ConnectAsync(factory.RedisConnectionString);
        var ttl = await redis.GetDatabase().KeyTimeToLiveAsync(pendingKey);
        ttl.Should().NotBeNull();
        ttl!.Value.Should().BeCloseTo(TimeSpan.FromHours(24), TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task Registering_a_duplicate_email_returns_409_conflict()
    {
        var client = factory.CreateClient();

        var first = await client.PostAsJsonAsync("/api/v1/auth/register", Body("duplicate@example.com"));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/v1/auth/register", Body("duplicate@example.com"));
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        second.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Registering_with_an_invalid_body_returns_422_unprocessable_entity()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register", Body("invalid@example.com", password: "weak"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
