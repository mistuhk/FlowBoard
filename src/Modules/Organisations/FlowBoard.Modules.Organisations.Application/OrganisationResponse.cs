using FlowBoard.Modules.Organisations.Domain.Aggregates;

namespace FlowBoard.Modules.Organisations.Application;

/// <summary>
/// The public representation of an organisation returned to API callers.
/// </summary>
/// <param name="Id">The organisation's identifier.</param>
/// <param name="Name">The organisation's display name.</param>
/// <param name="Slug">The organisation's unique, URL-safe slug.</param>
/// <param name="OwnerId">The identifier of the organisation's owner.</param>
public sealed record OrganisationResponse(Guid Id, string Name, string Slug, Guid OwnerId)
{
    /// <summary>Maps an <see cref="Organisation"/> aggregate to its response representation.</summary>
    /// <param name="organisation">The organisation to map.</param>
    public static OrganisationResponse From(Organisation organisation) =>
        new(
            organisation.Id.Value,
            organisation.Name.Value,
            organisation.Slug.Value,
            organisation.OwnerId.Value);
}
