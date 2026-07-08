using FlowBoard.Application.Abstractions;

namespace FlowBoard.Modules.Organisations.Application.Queries.ListOrganisations;

/// <summary>Lists the organisations the current user belongs to.</summary>
public sealed record ListOrganisationsQuery : IQuery<IReadOnlyList<OrganisationSummaryResponse>>;
