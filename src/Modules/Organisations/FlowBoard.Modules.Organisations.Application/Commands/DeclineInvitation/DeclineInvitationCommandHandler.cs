using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.DeclineInvitation;

/// <summary>
/// Handles <see cref="DeclineInvitationCommand"/>: resolves the organisation by the token hash and
/// removes the pending invitation. The aggregate enforces that the invitation is still pending.
/// </summary>
public sealed class DeclineInvitationCommandHandler(
    IOrganisationRepository organisations)
    : IRequestHandler<DeclineInvitationCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        DeclineInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = InvitationTokens.Hash(request.Token);

        var organisation = await organisations.GetByInvitationTokenHashAsync(tokenHash, cancellationToken);
        if (organisation is null)
            return Result.Failure(OrganisationErrors.InvalidInvitation);

        organisation.DeclineInvitation(tokenHash);

        return Result.Success();
    }
}
