using System.Reflection;

namespace FlowBoard.Modules.Search.Application;

/// <summary>
/// Marker exposing the Search Application assembly so the central MediatR scan discovers this module's
/// query handlers once the module is registered.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The Search Application assembly.</summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
