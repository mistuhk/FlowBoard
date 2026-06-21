namespace FlowBoard.Modules.Organisations.Presentation.Contracts;

/// <summary>
/// Request body for transferring organisation ownership.
/// </summary>
/// <param name="NewOwnerId">The member to transfer ownership to.</param>
public sealed record TransferOwnershipRequest(Guid NewOwnerId);
