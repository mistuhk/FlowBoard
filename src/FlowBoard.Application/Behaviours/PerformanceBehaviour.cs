using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowBoard.Application.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that emits a warning log entry for any handler
/// whose execution time exceeds <c>500ms</c>. Used to surface slow queries and
/// expensive operations without impacting normal request flow.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
public sealed class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await next(cancellationToken);
        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            logger.LogWarning(
                "Slow request detected: {RequestName} took {ElapsedMs}ms (threshold: {Threshold}ms)",
                typeof(TRequest).Name, sw.ElapsedMilliseconds, SlowRequestThresholdMs);
        }

        return response;
    }
}
