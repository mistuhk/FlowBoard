using System.Reflection;

namespace FlowBoard.Modules.Notifications.Application;

/// <summary>
/// Marker exposing the Notifications Application assembly so the central MediatR and FluentValidation
/// scans discover this module's handlers and validators once the module is registered.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The Notifications Application assembly.</summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
