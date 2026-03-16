using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Application.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that logs the entry, exit, and wall-clock duration
/// of every request. Registered first in the pipeline so it wraps all other behaviours
/// and provides end-to-end timing including validation and transaction overhead.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
public sealed class LoggingBehaviour<TRequest, TResponse>(
    ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            sw.Stop();

            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            logger.LogError(ex,
                "Error handling {RequestName} after {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);

            throw;
        }
    }
}
