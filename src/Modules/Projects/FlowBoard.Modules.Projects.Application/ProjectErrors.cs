using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Projects.Application;

/// <summary>
/// Well-known <see cref="Error"/> values returned by Projects command and query handlers.
/// </summary>
public static class ProjectErrors
{
    /// <summary>Returned when the requested project does not exist in the caller's organisation.</summary>
    public static readonly Error NotFound = new(
        "Projects.NotFound",
        "The project could not be found.");
}
