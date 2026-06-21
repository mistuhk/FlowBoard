using System.Reflection;

namespace FlowBoard.Modules.Projects.Application;

/// <summary>
/// Marker exposing the Projects Application assembly. Referencing <see cref="Assembly"/> from the
/// module's infrastructure registration forces this assembly to load, so the central MediatR and
/// FluentValidation scans in <c>Program.cs</c> discover its handlers and validators.
/// </summary>
public static class AssemblyReference
{
    /// <summary>The Projects Application assembly.</summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
