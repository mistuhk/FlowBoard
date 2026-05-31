using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Api.Services;

/// <summary>
/// No-op implementation of <see cref="ICurrentUserService"/> for Phase 0.
/// Replaced by the real HTTP context implementation in Phase 1.
/// </summary>
internal sealed class NullCurrentUserService : ICurrentUserService
{
    /// <inheritdoc/>
    public UserId UserId => UserId.From(Guid.Empty);

    /// <inheritdoc/>
    public string Email => string.Empty;

    /// <inheritdoc/>
    public bool IsAuthenticated => false;
}
