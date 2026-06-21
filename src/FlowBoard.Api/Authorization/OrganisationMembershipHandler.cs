using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;

namespace FlowBoard.Api.Authorization;

/// <summary>
/// Authorises <see cref="OrganisationMembershipRequirement"/> by resolving the caller's role in the
/// route organisation (via <see cref="IOrganisationMembershipReader"/>) and comparing it against the
/// required minimum role. Role ranking lives in the Organisations domain (<see cref="MemberRole"/>),
/// so downstream modules never need to interpret roles.
/// </summary>
public sealed class OrganisationMembershipHandler(
    ICurrentUserService currentUser,
    IOrganisationMembershipReader membershipReader,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<OrganisationMembershipRequirement>
{
    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationMembershipRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null || !currentUser.IsAuthenticated)
            return;

        if (!httpContext.Request.RouteValues.TryGetValue("orgId", out var raw)
            || !Guid.TryParse(raw?.ToString(), out var organisationId))
        {
            return;
        }

        var roleName = await membershipReader.GetRoleNameAsync(
            OrganisationId.From(organisationId),
            currentUser.UserId,
            httpContext.RequestAborted);

        if (roleName is null)
            return;

        if (MemberRole.FromPersistence(roleName).IsAtLeast(requirement.MinimumRole))
            context.Succeed(requirement);
    }
}
