using FlowBoard.Modules.Tasks.Application.Dashboard;

namespace FlowBoard.Modules.Tasks.Application.Abstractions;

/// <summary>
/// Read-optimised source for a user's dashboard. Implemented in the Tasks Infrastructure layer over
/// keyless read models (CQRS read side), so it composes task, project, and activity data without
/// depending on the other modules' aggregates.
/// </summary>
public interface IDashboardReader
{
    /// <summary>Builds the dashboard snapshot for a user, spanning every organisation they belong to.</summary>
    /// <param name="userId">The user whose dashboard to build.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<DashboardResponse> GetAsync(Guid userId, CancellationToken cancellationToken = default);
}
