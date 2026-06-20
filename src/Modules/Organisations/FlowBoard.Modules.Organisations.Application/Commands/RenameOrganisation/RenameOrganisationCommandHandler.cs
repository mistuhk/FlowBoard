using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.RenameOrganisation;

/// <summary>
/// Handles <see cref="RenameOrganisationCommand"/>: loads the organisation and applies the new
/// name. The aggregate enforces that only the owner may rename it. The change is persisted by the
/// unit of work when the surrounding transaction commits.
/// </summary>
public sealed class RenameOrganisationCommandHandler(
    ICurrentUserService currentUser,
    IOrganisationRepository organisations)
    : IRequestHandler<RenameOrganisationCommand, Result<OrganisationResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<OrganisationResponse>> Handle(
        RenameOrganisationCommand request,
        CancellationToken cancellationToken)
    {
        var organisation = await organisations.GetByIdAsync(
            OrganisationId.From(request.OrganisationId), cancellationToken);

        if (organisation is null)
            return Result.Failure<OrganisationResponse>(OrganisationErrors.NotFound);

        organisation.Rename(OrganisationName.Create(request.Name), currentUser.UserId);

        return Result.Success(OrganisationResponse.From(organisation));
    }
}
