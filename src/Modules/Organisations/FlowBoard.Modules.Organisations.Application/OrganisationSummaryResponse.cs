namespace FlowBoard.Modules.Organisations.Application;

/// <summary>An organisation the caller belongs to, including the caller's role in it.</summary>
/// <param name="Id">The organisation id.</param>
/// <param name="Name">The display name.</param>
/// <param name="Slug">The unique slug.</param>
/// <param name="OwnerId">The owner's user id.</param>
/// <param name="Role">The caller's role in this organisation.</param>
public sealed record OrganisationSummaryResponse(Guid Id, string Name, string Slug, Guid OwnerId, string Role);

/// <summary>A member of an organisation, with their user details and role.</summary>
/// <param name="UserId">The member's user id.</param>
/// <param name="Email">The member's email.</param>
/// <param name="DisplayName">The member's display name.</param>
/// <param name="Role">The member's role.</param>
/// <param name="JoinedAt">When they joined.</param>
public sealed record MemberResponse(Guid UserId, string Email, string DisplayName, string Role, DateTime JoinedAt);
