using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Application.Abstractions;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Queries.ListMembers;

/// <summary>
/// Handles <see cref="ListMembersQuery"/>: returns the organisation's members, but only to a caller
/// who is themselves a member (otherwise not-found). Read-only.
/// </summary>
public sealed class ListMembersQueryHandler(
    ICurrentUserService currentUser,
    IOrganisationMembershipReader membershipReader,
    IOrganisationReader reader)
    : IRequestHandler<ListMembersQuery, Result<IReadOnlyList<MemberResponse>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<MemberResponse>>> Handle(
        ListMembersQuery request, CancellationToken cancellationToken)
    {
        var callerRole = await membershipReader.GetRoleNameAsync(
            OrganisationId.From(request.OrganisationId), currentUser.UserId, cancellationToken);

        if (callerRole is null)
            return Result.Failure<IReadOnlyList<MemberResponse>>(OrganisationErrors.NotFound);

        var members = await reader.ListMembersAsync(request.OrganisationId, cancellationToken);
        return Result.Success(members);
    }
}
