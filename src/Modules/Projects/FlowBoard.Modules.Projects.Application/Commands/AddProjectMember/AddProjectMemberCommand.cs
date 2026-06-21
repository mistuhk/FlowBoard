using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application.Commands.AddProjectMember;

/// <summary>
/// Adds an organisation member as an explicit member of a project (used for Guest access).
/// </summary>
/// <param name="ProjectId">The project to add the member to.</param>
/// <param name="UserId">The user to add. Must be a member of the organisation.</param>
public sealed record AddProjectMemberCommand(Guid ProjectId, Guid UserId) : ICommand<Result>;
