using MediatR;

namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Marker interface for queries, read-only operations that do not modify state.
/// The <c>TransactionBehaviour</c> skips <see cref="IQuery{TResponse}"/> requests,
/// so no database transaction is opened. Query handlers should use read-optimised
/// paths (e.g. raw SQL via Dapper or EF Core <c>AsNoTracking</c>).
/// </summary>
/// <typeparam name="TResponse">The type of data returned by the query.</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>
{
}
