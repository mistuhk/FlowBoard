using FlowBoard.Application.Abstractions;
using MediatR;

namespace FlowBoard.Application.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that wraps <see cref="ICommand"/> and
/// <see cref="ICommand{TResponse}"/> handlers in an explicit database transaction.
/// <para>
/// The explicit transaction is required so that:
/// <list type="bullet">
///   <item>Aggregate changes and outbox messages are committed atomically.</item>
///   <item><c>SET LOCAL app.current_organisation_id</c> can be issued within the
///         transaction, enabling PostgreSQL Row-Level Security enforcement.</item>
/// </list>
/// </para>
/// <see cref="IQuery{TResponse}"/> requests pass straight through with no transaction overhead.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
public sealed class TransactionBehaviour<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Queries are read-only — skip transaction entirely
        if (request is IQuery<TResponse>)
            return await next(cancellationToken);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return response;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
