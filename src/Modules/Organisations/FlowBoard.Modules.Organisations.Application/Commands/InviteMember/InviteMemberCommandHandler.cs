using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.InviteMember;

/// <summary>
/// Handles <see cref="InviteMemberCommand"/>: loads the organisation, issues a single-use token,
/// and records the invitation on the aggregate. The aggregate enforces that only an Admin or the
/// Owner may invite and that the role is invitable. Only the token hash is stored; the raw token
/// is returned for delivery to the invitee.
/// </summary>
public sealed class InviteMemberCommandHandler(
    ICurrentUserService currentUser,
    IOrganisationRepository organisations)
    : IRequestHandler<InviteMemberCommand, Result<InvitationResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<InvitationResponse>> Handle(
        InviteMemberCommand request,
        CancellationToken cancellationToken)
    {
        var organisation = await organisations.GetByIdAsync(
            OrganisationId.From(request.OrganisationId), cancellationToken);

        if (organisation is null)
            return Result.Failure<InvitationResponse>(OrganisationErrors.NotFound);

        var role = MemberRole.FromPersistence(request.Role);
        var token = InvitationTokens.Generate();
        var tokenHash = InvitationTokens.Hash(token);

        organisation.InviteMember(request.Email, role, currentUser.UserId, tokenHash);

        var invitation = organisation.Invitations.First(i => i.TokenHash == tokenHash);

        return Result.Success(new InvitationResponse(
            invitation.Id.Value,
            organisation.Id.Value,
            invitation.InvitedEmail,
            invitation.Role.Name,
            invitation.ExpiresAt,
            token));
    }
}
