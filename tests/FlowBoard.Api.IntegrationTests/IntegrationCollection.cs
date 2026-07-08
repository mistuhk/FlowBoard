using Xunit;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Shares a single <see cref="FlowBoardApiFactory"/> (one Postgres + Redis container pair, one API
/// host) across every integration test class. Classes in the collection run sequentially, so the
/// suite no longer spins a container pair per class in parallel, which previously exhausted the CI
/// runner and failed the shared fixture. Tests use fresh, unique data so sharing is safe.
/// </summary>
[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<FlowBoardApiFactory>;
