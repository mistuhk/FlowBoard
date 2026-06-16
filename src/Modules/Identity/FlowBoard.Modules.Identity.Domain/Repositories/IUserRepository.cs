using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.ValueObjects;

namespace FlowBoard.Modules.Identity.Domain.Repositories;

/// <summary>
/// Persistence abstraction for the <see cref="User"/> aggregate.
/// Implemented in the Identity Infrastructure layer; the Domain owns the contract.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Determines whether a user already exists with the given email address.
    /// Email comparison is case-insensitive (the <see cref="Email"/> value object is normalised).
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if a user with that email already exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked <see cref="User"/> by its identifier, or <c>null</c> if no active
    /// (non-deleted) user exists with that id. The entity is change-tracked so callers can
    /// mutate it and have the change persisted by the unit of work.
    /// </summary>
    /// <param name="id">The identifier of the user to load.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a newly created <see cref="User"/> to the unit of work. The change is not
    /// persisted until the surrounding transaction calls <c>SaveChangesAsync</c>.
    /// </summary>
    /// <param name="user">The user aggregate to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
