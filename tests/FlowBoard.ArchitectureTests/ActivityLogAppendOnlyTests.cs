using System.Reflection;
using FlowBoard.Modules.ActivityLog.Domain;
using Xunit;

namespace FlowBoard.ArchitectureTests;

/// <summary>
/// Enforces that the activity log is append-only. The entity exposes no public setters and the
/// repository exposes no update or delete operations, so EF Core only ever generates INSERT and
/// SELECT for <c>activity_logs</c>, never UPDATE or DELETE.
/// </summary>
public class ActivityLogAppendOnlyTests
{
    [Fact(DisplayName = "ActivityLogEntry exposes no public property setters (append-only)")]
    public void Entry_has_no_public_setters()
    {
        var publicSetters = typeof(ActivityLogEntry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod is { IsPublic: true })
            .Select(p => p.Name)
            .ToList();

        Assert.Empty(publicSetters);
    }

    [Fact(DisplayName = "ActivityLogEntry has no mutating or deleting methods")]
    public void Entry_has_no_mutation_methods()
    {
        var mutators = typeof(ActivityLogEntry)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name)
            .ToList();

        Assert.Empty(mutators);
    }

    [Fact(DisplayName = "IActivityLogRepository exposes no update or delete operations")]
    public void Repository_has_no_update_or_delete()
    {
        var mutators = typeof(IActivityLogRepository)
            .GetMethods()
            .Where(m => m.Name.Contains("Update") || m.Name.Contains("Delete") || m.Name.Contains("Remove"))
            .Select(m => m.Name)
            .ToList();

        Assert.Empty(mutators);
    }
}
