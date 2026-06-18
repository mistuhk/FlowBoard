using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Application.Queries.GetUserProfile;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Commands.UpdateUserProfile;

/// <summary>
/// Handles <see cref="UpdateUserProfileCommand"/>: loads the authenticated user, applies the new
/// display name, and returns the updated profile. The change is persisted by the unit of work when
/// the surrounding transaction commits.
/// </summary>
public sealed class UpdateUserProfileCommandHandler(
    ICurrentUserService currentUser,
    IUserRepository users)
    : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<UserProfileResponse>> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(currentUser.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<UserProfileResponse>(IdentityErrors.UserNotFound);

        user.UpdateProfile(DisplayName.Create(request.DisplayName));

        return Result.Success(UserProfileResponse.From(user));
    }
}
