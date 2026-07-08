using FlowBoard.Application.Abstractions;
using FlowBoard.Modules.Tasks.Application.Dashboard;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetDashboard;

/// <summary>Returns the current user's dashboard snapshot (served from cache when warm).</summary>
public sealed record GetDashboardQuery : IQuery<DashboardResponse>;
