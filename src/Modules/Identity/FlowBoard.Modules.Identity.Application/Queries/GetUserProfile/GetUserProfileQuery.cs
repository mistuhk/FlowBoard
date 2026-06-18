using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Identity.Application.Queries.GetUserProfile;

/// <summary>
/// Reads the authenticated user's own profile. The user is taken from the request's identity, so
/// the query carries no parameters.
/// </summary>
public sealed record GetUserProfileQuery : IQuery<Result<UserProfileResponse>>;
