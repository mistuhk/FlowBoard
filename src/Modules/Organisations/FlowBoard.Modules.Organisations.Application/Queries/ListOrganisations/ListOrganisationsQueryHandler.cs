using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Organisations.Application.Abstractions;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Queries.ListOrganisations;

/// <summary>Handles <see cref="ListOrganisationsQuery"/>: the current user's organisations. Read-only.</summary>
public sealed class ListOrganisationsQueryHandler(
    ICurrentUserService currentUser,
    IOrganisationReader reader)
    : IRequestHandler<ListOrganisationsQuery, IReadOnlyList<OrganisationSummaryResponse>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<OrganisationSummaryResponse>> Handle(
        ListOrganisationsQuery request, CancellationToken cancellationToken) =>
        await reader.ListForUserAsync(currentUser.UserId.Value, cancellationToken);
}
