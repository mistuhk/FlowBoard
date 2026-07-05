using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Queries.ListMembers;

/// <summary>Lists the members of an organisation. The caller must be a member of it.</summary>
/// <param name="OrganisationId">The organisation whose members to list.</param>
public sealed record ListMembersQuery(Guid OrganisationId) : IQuery<Result<IReadOnlyList<MemberResponse>>>;
