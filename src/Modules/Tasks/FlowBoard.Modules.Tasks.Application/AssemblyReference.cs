using System.Reflection;

namespace FlowBoard.Modules.Tasks.Application;

/// <summary>
/// Marker exposing the Tasks Application assembly so the central MediatR and FluentValidation scans
/// discover this module's handlers and validators once the module is registered.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The Tasks Application assembly.</summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
