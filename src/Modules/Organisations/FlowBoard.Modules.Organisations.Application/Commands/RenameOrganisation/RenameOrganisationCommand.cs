using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.RenameOrganisation;

/// <summary>
/// Renames an existing organisation. Only the owner may rename it.
/// </summary>
/// <param name="OrganisationId">The identifier of the organisation to rename.</param>
/// <param name="Name">The new display name. Must be 2 to 100 characters.</param>
public sealed record RenameOrganisationCommand(Guid OrganisationId, string Name)
    : ICommand<Result<OrganisationResponse>>;
