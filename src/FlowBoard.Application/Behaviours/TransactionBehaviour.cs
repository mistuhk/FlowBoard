using FlowBoard.Application.Abstractions;
using MediatR;

namespace FlowBoard.Application.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that wraps <see cref="ICommand"/> and
/// <see cref="ICommand{TResponse}"/> handlers in a database transaction.
/// <see cref="IQuery{TResponse}"/> requests pass straight through with no transaction overhead.
/// On successful completion the <see cref="IUnitOfWork.SaveChangesAsync"/> is called,
/// which commits pending EF Core changes and serialises domain events to the outbox.
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
        if (request is not ICommand and not ICommand<TResponse>)
            return await next();

        var response = await next();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}
