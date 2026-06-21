using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.DeleteOrganisation;

/// <summary>
/// Handles <see cref="DeleteOrganisationCommand"/>: loads the organisation and soft-deletes it.
/// The aggregate enforces that only the owner may delete it. The change is persisted by the unit
/// of work when the surrounding transaction commits.
/// </summary>
public sealed class DeleteOrganisationCommandHandler(
    ICurrentUserService currentUser,
    IOrganisationRepository organisations)
    : IRequestHandler<DeleteOrganisationCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        DeleteOrganisationCommand request,
        CancellationToken cancellationToken)
    {
        var organisation = await organisations.GetByIdAsync(
            OrganisationId.From(request.OrganisationId), cancellationToken);

        if (organisation is null)
            return Result.Failure(OrganisationErrors.NotFound);

        organisation.Delete(currentUser.UserId);

        return Result.Success();
    }
}
