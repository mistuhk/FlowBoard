using FlowBoard.Infrastructure.Outbox;
using FlowBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Integration tests for the transactional outbox dispatcher. Commands persist domain events to the
/// outbox; the processor publishes them to their handlers and stamps them processed. This class has
/// its own database (one factory per class), so it owns every outbox row it asserts on.
/// </summary>
public sealed class OutboxProcessorTests(FlowBoardApiFactory factory)
    : IClassFixture<FlowBoardApiFactory>
{
    private async Task RunProcessorAsync()
    {
        using var scope = factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
        await processor.RunAsync();
    }

    [Fact]
    public async Task Dispatches_persisted_events_and_dead_letters_unresolvable_ones()
    {
        // Registering and verifying a user persists domain events to the outbox.
        var client = factory.CreateClient();
        await AuthTestHelpers.RegisterAndVerifyAsync(factory, client, "outbox@example.com");

        await RunProcessorAsync();

        // Every persisted event has now been dispatched.
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Set<OutboxMessage>().CountAsync(m => m.ProcessedAt != null)).Should().BeGreaterThan(0);
            (await db.Set<OutboxMessage>().CountAsync(m => m.ProcessedAt == null)).Should().Be(0);
        }

        // A message whose event type cannot be resolved is recorded as failed and left for retry.
        var poisonId = Guid.NewGuid();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Set<OutboxMessage>().Add(new OutboxMessage
            {
                Id = poisonId,
                AggregateType = "Unknown",
                AggregateId = Guid.NewGuid(),
                EventType = "FlowBoard.Does.Not.Exist",
                Payload = "{}",
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        await RunProcessorAsync();

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var poison = await db.Set<OutboxMessage>().FirstAsync(m => m.Id == poisonId);
            poison.ProcessedAt.Should().BeNull();
            poison.Error.Should().NotBeNullOrEmpty();
        }
    }
}
