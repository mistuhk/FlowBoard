using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.TransferOwnership;

/// <summary>
/// Transfers ownership of an organisation to another existing member. Only the current owner may
/// perform this; the previous owner is demoted to Admin.
/// </summary>
/// <param name="OrganisationId">The organisation whose ownership is changing.</param>
/// <param name="NewOwnerId">The member to transfer ownership to.</param>
public sealed record TransferOwnershipCommand(Guid OrganisationId, Guid NewOwnerId)
    : ICommand<Result>;
