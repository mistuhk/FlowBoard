using FluentValidation;
using MediatR;

namespace FlowBoard.Application.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that runs all registered <see cref="IValidator{T}"/>
/// implementations for a request before the handler executes.
/// Throws <see cref="ValidationException"/> on failure, which is caught by
/// <c>ExceptionHandlingMiddleware</c> and returned as an HTTP 422 response.
/// Requests with no registered validators pass straight through.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
public sealed class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }
}
