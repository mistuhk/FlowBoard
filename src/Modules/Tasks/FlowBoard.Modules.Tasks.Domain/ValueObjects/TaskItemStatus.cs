using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Tasks.Domain.ValueObjects;

/// <summary>
/// Value object for a task's status, including the allowed state-machine transitions.
/// Named <c>TaskItemStatus</c> (not <c>TaskStatus</c>) to avoid colliding with
/// <see cref="System.Threading.Tasks.TaskStatus"/>. Persisted as a lowercase token.
/// </summary>
public sealed class TaskItemStatus : ValueObject
{
    /// <summary>Newly created, not yet started.</summary>
    public static readonly TaskItemStatus Todo = new(nameof(Todo), "todo");

    /// <summary>Actively being worked on.</summary>
    public static readonly TaskItemStatus InProgress = new(nameof(InProgress), "in_progress");

    /// <summary>Blocked on a dependency.</summary>
    public static readonly TaskItemStatus Blocked = new(nameof(Blocked), "blocked");

    /// <summary>Completed.</summary>
    public static readonly TaskItemStatus Done = new(nameof(Done), "done");

    // Allowed transitions: Todo -> InProgress/Blocked; InProgress -> Blocked/Done/Todo;
    // Blocked -> InProgress/Todo; Done -> Todo (reopen).
    private static readonly Dictionary<string, TaskItemStatus[]> AllowedTransitions = new()
    {
        [Todo.Name] = [InProgress, Blocked],
        [InProgress.Name] = [Blocked, Done, Todo],
        [Blocked.Name] = [InProgress, Todo],
        [Done.Name] = [Todo],
    };

    private TaskItemStatus(string name, string dbValue)
    {
        Name = name;
        DbValue = dbValue;
    }

    /// <summary>The status name (for example <c>InProgress</c>), used for display and equality.</summary>
    public string Name { get; }

    /// <summary>The persisted token (for example <c>in_progress</c>).</summary>
    public string DbValue { get; }

    /// <summary>All defined statuses.</summary>
    public static IReadOnlyList<TaskItemStatus> All { get; } = [Todo, InProgress, Blocked, Done];

    /// <summary>Returns <c>true</c> if a transition from this status to <paramref name="target"/> is allowed.</summary>
    /// <param name="target">The status being transitioned to.</param>
    public bool CanTransitionTo(TaskItemStatus target) => AllowedTransitions[Name].Contains(target);

    /// <summary>Rehydrates a <see cref="TaskItemStatus"/> from its persisted token.</summary>
    /// <param name="dbValue">The stored status token.</param>
    /// <exception cref="DomainException">Thrown if no status matches the token.</exception>
    public static TaskItemStatus FromPersistence(string dbValue) =>
        All.FirstOrDefault(status => status.DbValue == dbValue)
        ?? throw new DomainException($"'{dbValue}' is not a recognised task status.");

    /// <summary>Resolves a <see cref="TaskItemStatus"/> from its name (case-insensitive), as used by the API.</summary>
    /// <param name="name">The status name, for example <c>InProgress</c>.</param>
    /// <exception cref="DomainException">Thrown if no status matches the name.</exception>
    public static TaskItemStatus FromName(string name) =>
        All.FirstOrDefault(status => string.Equals(status.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new DomainException($"'{name}' is not a recognised task status.");

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
