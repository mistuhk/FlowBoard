using System.Reflection;

namespace FlowBoard.Modules.Organisations.Application;

/// <summary>
/// Marker exposing the Organisations Application assembly. Referencing <see cref="Assembly"/>
/// from the module's infrastructure registration forces this assembly to load, so the central
/// MediatR and FluentValidation scans in <c>Program.cs</c> discover its handlers and validators
/// (the scans only see assemblies already loaded into the AppDomain).
/// </summary>
public static class AssemblyReference
{
    /// <summary>The Organisations Application assembly.</summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
