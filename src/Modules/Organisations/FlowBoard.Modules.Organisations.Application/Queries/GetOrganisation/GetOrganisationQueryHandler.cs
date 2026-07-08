using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Organisations.Application.Abstractions;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Queries.GetOrganisation;

/// <summary>
/// Handles <see cref="GetOrganisationQuery"/>: returns the organisation only if the caller is a
/// member, otherwise not-found (which also avoids disclosing organisations the caller cannot see).
/// Read-only.
/// </summary>
public sealed class GetOrganisationQueryHandler(
    ICurrentUserService currentUser,
    IOrganisationReader reader)
    : IRequestHandler<GetOrganisationQuery, Result<OrganisationSummaryResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<OrganisationSummaryResponse>> Handle(
        GetOrganisationQuery request, CancellationToken cancellationToken)
    {
        var organisation = await reader.GetForUserAsync(currentUser.UserId.Value, request.OrganisationId, cancellationToken);
        return organisation is null
            ? Result.Failure<OrganisationSummaryResponse>(OrganisationErrors.NotFound)
            : Result.Success(organisation);
    }
}
