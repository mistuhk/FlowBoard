using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Application.Queries.GetUserProfile;

namespace FlowBoard.Modules.Identity.Application.Commands.UpdateUserProfile;

/// <summary>
/// Updates the authenticated user's editable profile details. The user is taken from the request's
/// identity, not the payload.
/// </summary>
/// <param name="DisplayName">The new display name.</param>
public sealed record UpdateUserProfileCommand(string DisplayName)
    : ICommand<Result<UserProfileResponse>>;
