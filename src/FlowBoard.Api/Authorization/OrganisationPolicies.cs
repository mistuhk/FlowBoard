namespace FlowBoard.Api.Authorization;

/// <summary>
/// Names of the organisation-scoped authorisation policies. Controllers for org-owned resources
/// decorate their actions with these via <c>[Authorize(Policy = ...)]</c>.
/// </summary>
public static class OrganisationPolicies
{
    /// <summary>Requires the caller to be an active member of the route organisation (any role).</summary>
    public const string Member = "OrganisationMember";

    /// <summary>Requires the caller to be an Admin or the Owner of the route organisation.</summary>
    public const string Admin = "OrganisationAdmin";
}
