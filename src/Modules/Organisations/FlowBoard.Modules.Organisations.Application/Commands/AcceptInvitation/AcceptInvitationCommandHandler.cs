using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.AcceptInvitation;

/// <summary>
/// Handles <see cref="AcceptInvitationCommand"/>: resolves the organisation by the token hash and
/// joins the authenticated user. The aggregate enforces that the invitation is still pending and
/// that the user is not already a member.
/// </summary>
public sealed class AcceptInvitationCommandHandler(
    ICurrentUserService currentUser,
    IOrganisationRepository organisations)
    : IRequestHandler<AcceptInvitationCommand, Result<OrganisationResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<OrganisationResponse>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = InvitationTokens.Hash(request.Token);

        var organisation = await organisations.GetByInvitationTokenHashAsync(tokenHash, cancellationToken);
        if (organisation is null)
            return Result.Failure<OrganisationResponse>(OrganisationErrors.InvalidInvitation);

        organisation.AcceptInvitation(tokenHash, currentUser.UserId);

        return Result.Success(OrganisationResponse.From(organisation));
    }
}
