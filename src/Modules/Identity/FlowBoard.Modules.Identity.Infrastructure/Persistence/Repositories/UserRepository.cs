using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Identity.Domain.Aggregates;
using FlowBoard.Modules.Identity.Domain.Repositories;
using FlowBoard.Modules.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>. Writes are buffered on the
/// shared <see cref="AppDbContext"/> and flushed by the unit of work when the surrounding
/// transaction commits.
/// </summary>
internal sealed class UserRepository(AppDbContext context) : IUserRepository
{
    /// <inheritdoc/>
    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        context.Set<User>().AnyAsync(u => u.Email == email, cancellationToken);

    /// <inheritdoc/>
    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        context.Set<User>().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    /// <inheritdoc/>
    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        context.Set<User>().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    /// <inheritdoc/>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await context.Set<User>().AddAsync(user, cancellationToken);
}
