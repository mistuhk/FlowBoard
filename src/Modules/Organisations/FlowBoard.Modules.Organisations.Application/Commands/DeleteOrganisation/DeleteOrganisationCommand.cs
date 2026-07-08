using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.DeleteOrganisation;

/// <summary>
/// Soft-deletes an organisation. Only the owner may delete it.
/// </summary>
/// <param name="OrganisationId">The identifier of the organisation to delete.</param>
public sealed record DeleteOrganisationCommand(Guid OrganisationId)
    : ICommand<Result>;
