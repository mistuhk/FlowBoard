using System.Reflection;

namespace FlowBoard.Modules.ActivityLog.Application;

/// <summary>
/// Marker exposing the ActivityLog Application assembly so the central MediatR and FluentValidation
/// scans discover this module's event handlers and validators once the module is registered.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The ActivityLog Application assembly.</summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
