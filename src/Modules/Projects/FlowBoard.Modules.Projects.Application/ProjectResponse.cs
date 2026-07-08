using FlowBoard.Modules.Projects.Domain.Aggregates;

namespace FlowBoard.Modules.Projects.Application;

/// <summary>
/// The public representation of a project returned to API callers.
/// </summary>
/// <param name="Id">The project's identifier.</param>
/// <param name="OrganisationId">The owning organisation.</param>
/// <param name="Name">The project's name.</param>
/// <param name="Description">The project's description, if any.</param>
/// <param name="Status">The project's status name (Active or Archived).</param>
/// <param name="CreatedById">The user who created the project.</param>
public sealed record ProjectResponse(
    Guid Id,
    Guid OrganisationId,
    string Name,
    string? Description,
    string Status,
    Guid CreatedById)
{
    /// <summary>Maps a <see cref="Project"/> aggregate to its response representation.</summary>
    /// <param name="project">The project to map.</param>
    public static ProjectResponse From(Project project) =>
        new(
            project.Id.Value,
            project.OrganisationId.Value,
            project.Name.Value,
            project.Description,
            project.Status.Name,
            project.CreatedById.Value);
}
