using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Commands.RemoveProjectMember;

/// <summary>Removes an explicit member from a project.</summary>
/// <param name="ProjectId">The project to remove the member from.</param>
/// <param name="UserId">The user to remove.</param>
public sealed record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : ICommand<Result>;
