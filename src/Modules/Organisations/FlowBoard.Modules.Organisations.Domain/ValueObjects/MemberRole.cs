using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.Exceptions;

namespace FlowBoard.Modules.Organisations.Domain.ValueObjects;

/// <summary>
/// Value object representing a member's role within an organisation.
/// Roles form a strict ordering by privilege: Owner &gt; Admin &gt; Member &gt; Guest.
/// </summary>
public sealed class MemberRole : ValueObject
{
    /// <summary>The organisation's sole owner. Holds the highest rank and cannot be removed.</summary>
    public static readonly MemberRole Owner = new(nameof(Owner), 4);

    /// <summary>An administrator who can manage members and invitations.</summary>
    public static readonly MemberRole Admin = new(nameof(Admin), 3);

    /// <summary>A standard member.</summary>
    public static readonly MemberRole Member = new(nameof(Member), 2);

    /// <summary>A limited-access guest. Holds the lowest rank.</summary>
    public static readonly MemberRole Guest = new(nameof(Guest), 1);

    private MemberRole(string name, int rank)
    {
        Name = name;
        Rank = rank;
    }

    /// <summary>The canonical name of the role, used as its persisted value.</summary>
    public string Name { get; }

    /// <summary>The privilege rank of the role. Higher values outrank lower ones.</summary>
    public int Rank { get; }

    /// <summary>All defined roles, ordered from highest to lowest rank.</summary>
    public static IReadOnlyList<MemberRole> All { get; } = [Owner, Admin, Member, Guest];

    /// <summary>Returns <c>true</c> if this role ranks at or above <paramref name="other"/>.</summary>
    /// <param name="other">The role to compare against.</param>
    public bool IsAtLeast(MemberRole other) => Rank >= other.Rank;

    /// <summary>Returns <c>true</c> if this role ranks strictly above <paramref name="other"/>.</summary>
    /// <param name="other">The role to compare against.</param>
    public bool Outranks(MemberRole other) => Rank > other.Rank;

    /// <summary>
    /// Resolves a <see cref="MemberRole"/> from its canonical name.
    /// </summary>
    /// <param name="name">The role name (case-sensitive), as produced by <see cref="Name"/>.</param>
    /// <returns>The matching <see cref="MemberRole"/>.</returns>
    /// <exception cref="DomainException">Thrown if no role matches the given name.</exception>
    public static MemberRole FromName(string name) =>
        All.FirstOrDefault(role => role.Name == name)
        ?? throw new DomainException($"'{name}' is not a recognised member role.");

    /// <summary>
    /// Rehydrates a <see cref="MemberRole"/> from its persisted token, matching
    /// case-insensitively. Roles are stored in lowercase (for example <c>owner</c>).
    /// </summary>
    /// <param name="value">The stored role token.</param>
    /// <returns>The matching <see cref="MemberRole"/>.</returns>
    /// <exception cref="DomainException">Thrown if no role matches the stored token.</exception>
    public static MemberRole FromPersistence(string value) =>
        All.FirstOrDefault(role => string.Equals(role.Name, value, StringComparison.OrdinalIgnoreCase))
        ?? throw new DomainException($"'{value}' is not a recognised member role.");

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
