using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.TransferOwnership;

/// <summary>
/// Handles <see cref="TransferOwnershipCommand"/>: loads the organisation and transfers ownership.
/// The aggregate enforces that only the current owner may transfer and that the new owner is a member.
/// </summary>
public sealed class TransferOwnershipCommandHandler(
    ICurrentUserService currentUser,
    IOrganisationRepository organisations)
    : IRequestHandler<TransferOwnershipCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(TransferOwnershipCommand request, CancellationToken cancellationToken)
    {
        var organisation = await organisations.GetByIdAsync(
            OrganisationId.From(request.OrganisationId), cancellationToken);

        if (organisation is null)
            return Result.Failure(OrganisationErrors.NotFound);

        organisation.TransferOwnership(UserId.From(request.NewOwnerId), currentUser.UserId);

        return Result.Success();
    }
}
