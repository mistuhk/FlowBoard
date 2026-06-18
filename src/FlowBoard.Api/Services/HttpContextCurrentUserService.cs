using System.Security.Claims;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Api.Services;

/// <summary>
/// Resolves the authenticated user from the current request's JWT claims via
/// <see cref="IHttpContextAccessor"/>. The access token carries the user id in the <c>sub</c> claim
/// and the address in the <c>email</c> claim (claim names are kept verbatim by the bearer setup).
/// </summary>
internal sealed class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    /// <inheritdoc/>
    public UserId UserId =>
        Guid.TryParse(Principal?.FindFirstValue("sub"), out var id)
            ? UserId.From(id)
            : throw new InvalidOperationException(
                "No authenticated user. UserId is only available on authenticated requests.");

    /// <inheritdoc/>
    public string Email => Principal?.FindFirstValue("email") ?? string.Empty;

    /// <inheritdoc/>
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
