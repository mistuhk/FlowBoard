using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.RemoveMember;

/// <summary>
/// Handles <see cref="RemoveMemberCommand"/>: loads the organisation and removes the member.
/// The aggregate enforces that the Owner cannot be removed and that the caller outranks the target.
/// </summary>
public sealed class RemoveMemberCommandHandler(
    ICurrentUserService currentUser,
    IOrganisationRepository organisations)
    : IRequestHandler<RemoveMemberCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var organisation = await organisations.GetByIdAsync(
            OrganisationId.From(request.OrganisationId), cancellationToken);

        if (organisation is null)
            return Result.Failure(OrganisationErrors.NotFound);

        organisation.RemoveMember(UserId.From(request.UserId), currentUser.UserId);

        return Result.Success();
    }
}
