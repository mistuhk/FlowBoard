using FlowBoard.Modules.Projects.Domain.Entities;

namespace FlowBoard.Modules.Projects.Application;

/// <summary>The public representation of an explicit project member.</summary>
/// <param name="UserId">The member's user id.</param>
/// <param name="AddedAt">When the user was added to the project.</param>
public sealed record ProjectMemberResponse(Guid UserId, DateTime AddedAt)
{
    /// <summary>Maps a <see cref="ProjectMember"/> entity to its response representation.</summary>
    /// <param name="member">The membership to map.</param>
    public static ProjectMemberResponse From(ProjectMember member) =>
        new(member.UserId.Value, member.AddedAt);
}
