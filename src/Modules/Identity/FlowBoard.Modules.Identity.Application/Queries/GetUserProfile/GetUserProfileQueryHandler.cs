using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Identity.Domain.Repositories;
using MediatR;

namespace FlowBoard.Modules.Identity.Application.Queries.GetUserProfile;

/// <summary>
/// Handles <see cref="GetUserProfileQuery"/>: loads the authenticated user and returns their
/// profile. Read-only, so it runs outside a database transaction.
/// </summary>
public sealed class GetUserProfileQueryHandler(
    ICurrentUserService currentUser,
    IUserRepository users)
    : IRequestHandler<GetUserProfileQuery, Result<UserProfileResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<UserProfileResponse>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(currentUser.UserId, cancellationToken);
        return user is null
            ? Result.Failure<UserProfileResponse>(IdentityErrors.UserNotFound)
            : Result.Success(UserProfileResponse.From(user));
    }
}
