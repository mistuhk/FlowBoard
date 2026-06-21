using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.ChangeMemberRole;

/// <summary>
/// Handles <see cref="ChangeMemberRoleCommand"/>: loads the organisation and applies the role
/// change. The aggregate enforces the outranking rules and that ownership is not assigned.
/// </summary>
public sealed class ChangeMemberRoleCommandHandler(
    ICurrentUserService currentUser,
    IOrganisationRepository organisations)
    : IRequestHandler<ChangeMemberRoleCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(ChangeMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var organisation = await organisations.GetByIdAsync(
            OrganisationId.From(request.OrganisationId), cancellationToken);

        if (organisation is null)
            return Result.Failure(OrganisationErrors.NotFound);

        organisation.ChangeMemberRole(
            UserId.From(request.UserId),
            MemberRole.FromPersistence(request.Role),
            currentUser.UserId);

        return Result.Success();
    }
}
