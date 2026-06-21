namespace FlowBoard.Domain.Shared.ValueObjects;

// Strongly-typed IDs prevent accidentally passing the wrong ID type across
// aggregate boundaries. Each is a readonly record struct — zero allocation
// overhead compared to a plain Guid, but with full type safety.

/// <summary>Strongly-typed identifier for a <c>User</c> aggregate.</summary>
public readonly record struct UserId(Guid Value)
{
    /// <summary>Creates a new <see cref="UserId"/> with a randomly generated value.</summary>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="UserId"/> from an existing <see cref="Guid"/>.</summary>
    public static UserId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for an <c>Organisation</c> aggregate.</summary>
public readonly record struct OrganisationId(Guid Value)
{
    /// <summary>Creates a new <see cref="OrganisationId"/> with a randomly generated value.</summary>
    public static OrganisationId New() => new(Guid.NewGuid());

    /// <summary>Creates an <see cref="OrganisationId"/> from an existing <see cref="Guid"/>.</summary>
    public static OrganisationId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <c>Project</c> aggregate.</summary>
public readonly record struct ProjectId(Guid Value)
{
    /// <summary>Creates a new <see cref="ProjectId"/> with a randomly generated value.</summary>
    public static ProjectId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="ProjectId"/> from an existing <see cref="Guid"/>.</summary>
    public static ProjectId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <c>Task</c> aggregate.</summary>
public readonly record struct TaskId(Guid Value)
{
    /// <summary>Creates a new <see cref="TaskId"/> with a randomly generated value.</summary>
    public static TaskId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="TaskId"/> from an existing <see cref="Guid"/>.</summary>
    public static TaskId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <c>Comment</c> entity.</summary>
public readonly record struct CommentId(Guid Value)
{
    /// <summary>Creates a new <see cref="CommentId"/> with a randomly generated value.</summary>
    public static CommentId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="CommentId"/> from an existing <see cref="Guid"/>.</summary>
    public static CommentId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <c>Notification</c> aggregate.</summary>
public readonly record struct NotificationId(Guid Value)
{
    /// <summary>Creates a new <see cref="NotificationId"/> with a randomly generated value.</summary>
    public static NotificationId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="NotificationId"/> from an existing <see cref="Guid"/>.</summary>
    public static NotificationId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <c>Membership</c> entity (child of <c>Organisation</c>).</summary>
public readonly record struct MembershipId(Guid Value)
{
    /// <summary>Creates a new <see cref="MembershipId"/> with a randomly generated value.</summary>
    public static MembershipId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="MembershipId"/> from an existing <see cref="Guid"/>.</summary>
    public static MembershipId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for an <c>Invitation</c> entity (child of <c>Organisation</c>).</summary>
public readonly record struct InvitationId(Guid Value)
{
    /// <summary>Creates a new <see cref="InvitationId"/> with a randomly generated value.</summary>
    public static InvitationId New() => new(Guid.NewGuid());

    /// <summary>Creates an <see cref="InvitationId"/> from an existing <see cref="Guid"/>.</summary>
    public static InvitationId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}

/// <summary>Strongly-typed identifier for a <c>ProjectMember</c> entity (child of <c>Project</c>).</summary>
public readonly record struct ProjectMemberId(Guid Value)
{
    /// <summary>Creates a new <see cref="ProjectMemberId"/> with a randomly generated value.</summary>
    public static ProjectMemberId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="ProjectMemberId"/> from an existing <see cref="Guid"/>.</summary>
    public static ProjectMemberId From(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}
