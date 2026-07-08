using FlowBoard.Domain.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Api.Middleware;

/// <summary>
/// ASP.NET Core middleware that catches all unhandled exceptions and maps them to
/// RFC 7807 <see cref="ProblemDetails"/> responses. Must be registered first in the
/// pipeline so it wraps every subsequent middleware and controller.
/// Stack traces are never included in responses to callers; all exceptions are logged
/// with their full detail including <c>TraceId</c>.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>Invokes the middleware, catching and mapping any unhandled exception.</summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await HandleAsync(context, ex);
        }
    }

    private static async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, type) = ex switch
        {
            ValidationException => (422, "Validation Failed", "validation-failed"),
            NotFoundException => (404, "Resource Not Found", "not-found"),
            ForbiddenException => (403, "Forbidden", "forbidden"),
            ConflictException => (409, "Conflict", "conflict"),
            DomainException => (422, "Business Rule Violation", "domain-error"),
            _ => (500, "Internal Server Error", "server-error"),
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://flowboard.io/errors/{type}",
            Extensions = { ["traceId"] = context.TraceIdentifier }
        };

        if (ex is ValidationException ve)
        {
            problem.Extensions["errors"] = ve.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
