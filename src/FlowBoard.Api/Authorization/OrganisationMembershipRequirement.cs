using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;

namespace FlowBoard.Api.Authorization;

/// <summary>
/// Authorisation requirement that the caller holds at least <see cref="MinimumRole"/> in the
/// organisation identified by the request's <c>orgId</c> route value.
/// </summary>
/// <param name="minimumRole">The lowest role that satisfies the requirement.</param>
public sealed class OrganisationMembershipRequirement(MemberRole minimumRole) : IAuthorizationRequirement
{
    /// <summary>The lowest role that satisfies this requirement.</summary>
    public MemberRole MinimumRole { get; } = minimumRole;
}
