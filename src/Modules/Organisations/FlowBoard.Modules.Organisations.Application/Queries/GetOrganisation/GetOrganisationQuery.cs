using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Queries.GetOrganisation;

/// <summary>Returns a single organisation the current user belongs to, with their role.</summary>
/// <param name="OrganisationId">The organisation to fetch.</param>
public sealed record GetOrganisationQuery(Guid OrganisationId) : IQuery<Result<OrganisationSummaryResponse>>;
