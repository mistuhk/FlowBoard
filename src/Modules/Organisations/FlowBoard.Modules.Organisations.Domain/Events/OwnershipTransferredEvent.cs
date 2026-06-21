using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Shared.ValueObjects;

namespace FlowBoard.Modules.Organisations.Domain.Events;

/// <summary>
/// Raised when organisation ownership is transferred via <c>Organisation.TransferOwnership</c>.
/// The previous owner is demoted to Admin and the new owner takes the Owner role.
/// </summary>
/// <param name="OrganisationId">The organisation whose ownership changed.</param>
/// <param name="FromUserId">The previous owner.</param>
/// <param name="ToUserId">The new owner.</param>
public sealed record OwnershipTransferredEvent(
    OrganisationId OrganisationId,
    UserId FromUserId,
    UserId ToUserId) : DomainEvent;
